using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class HalloweenExchangeManager : MonoBehaviour
{
    [Header("Offers")]
    [SerializeField]
    private HalloweenExchangeOffer[] offers =
        Array.Empty<HalloweenExchangeOffer>();

    [Header("Currency")]
    [SerializeField]
    private string currencyId = "Конфетыквы";

    [Header("Promo code")]
    [SerializeField, Min(1)]
    private int promoCodeLifetimeDays = 7;

    public event Action ExchangeChanged;

    public IReadOnlyList<HalloweenExchangeOffer> Offers => offers;

    public int PromoCodeLifetimeDays => Mathf.Max(1, promoCodeLifetimeDays);

    private void OnValidate()
    {
        promoCodeLifetimeDays = Mathf.Max(1, promoCodeLifetimeDays);

        if (!string.IsNullOrWhiteSpace(currencyId))
            currencyId = currencyId.Trim();
    }

    public int GetCurrencyBalance()
    {
        return ResourceInventoryManager.Instance != null
            ? ResourceInventoryManager.Instance.GetAmount(currencyId)
            : 0;
    }

    public int GetBasketCapacityUsed()
    {
        return AccountManager.Instance != null
            ? AccountManager.Instance.GetHalloweenBasketCapacityUsed()
            : 0;
    }

    public List<HalloweenPromoCodeRecord> GetPromoHistory()
    {
        return AccountManager.Instance != null
            ? AccountManager.Instance.GetHalloweenPromoCodeSnapshot()
            : new List<HalloweenPromoCodeRecord>();
    }

    public HalloweenExchangeOffer FindOffer(string offerId)
    {
        if (string.IsNullOrWhiteSpace(offerId) || offers == null)
            return null;

        foreach (HalloweenExchangeOffer offer in offers)
        {
            if (offer != null &&
                string.Equals(
                    offer.OfferId?.Trim(),
                    offerId.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return offer;
            }
        }

        return null;
    }

    public bool CanPurchase(HalloweenExchangeOffer offer,out string reason)
    {
        reason = string.Empty;

        if (offer == null || !offer.Available)
            return Fail("Это предложение сейчас недоступно.", out reason);

        if (string.IsNullOrWhiteSpace(offer.OfferId) ||
            string.IsNullOrWhiteSpace(offer.RewardInventoryId) ||
            offer.CandyPrice <= 0 ||
            offer.BasketCapacityPercent < 1 ||
            offer.BasketCapacityPercent > 100)
        {
            return Fail("У предложения некорректно настроены ID, цена или вместимость.", out reason);
        }

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory =
            ResourceInventoryManager.Instance;

        if (account == null || inventory == null)
            return Fail(
                "Системы аккаунта или инвентаря ещё не готовы.",
                out reason);

        InventoryEntryDefinition reward =
            inventory.GetDefinition(offer.RewardInventoryId);

        if (reward == null)
            return Fail(
                "Награда отсутствует в каталоге ResourceInventoryManager.",
                out reason);

        if (reward.type != InventoryEntryType.UniqueItem)
            return Fail(
                "Награда должна иметь тип UniqueItem.",
                out reason);

        if (inventory.IsExpired(reward.id))
            return Fail(
                "Срок действия этой награды закончился.",
                out reason);

        if (account.HasUniqueInventoryItem(reward.id))
            return Fail(
                "Этот уникальный предмет у тебя уже есть.",
                out reason);

        if (account.HasHalloweenPromoForReward(reward.id))
            return Fail(
                "Ты уже покупал промокод на этот предмет. Повторная покупка недоступна.",
                out reason);

        if (GetBasketCapacityUsed() +
            offer.BasketCapacityPercent > 100)
        {
            return Fail(
                "В корзине не хватает места для этой награды.",
                out reason);
        }

        if (GetCurrencyBalance() < offer.CandyPrice)
            return Fail(
                "Недостаточно Конфетыкв.",
                out reason);

        return true;
    }

    public bool TryPurchaseOffer(HalloweenExchangeOffer offer, out HalloweenPromoCodeRecord createdRecord, out string failureReason)
    {
        createdRecord = null;

        if (!CanPurchase(offer, out failureReason))
            return false;

        AccountManager account = AccountManager.Instance;

        if (!HalloweenPromoCodeService.TryGenerateUniqueCode(account, out string code))
        {
            return Fail("Не удалось создать уникальный промокод. Попробуй ещё раз.",out failureReason);
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (!HalloweenPromoCodeService.TryCalculateExpiry(now,PromoCodeLifetimeDays,out long expiresAt))
        {
            return Fail("Не удалось рассчитать срок действия промокода.",out failureReason);
        }

        bool purchased = account.TryPurchaseHalloweenOffer(
            currencyId,
            offer.OfferId,
            offer.RewardInventoryId,
            offer.CandyPrice,
            offer.BasketCapacityPercent,
            code,
            now,
            expiresAt,
            out failureReason);

        if (!purchased)
            return false;

        ResourceInventoryManager.Instance
            .NotifyAccountAmountChanged(currencyId);

        createdRecord = new HalloweenPromoCodeRecord
        {
            code = code,
            offerId = offer.OfferId,
            rewardInventoryId = offer.RewardInventoryId,
            pricePaid = offer.CandyPrice,
            basketCapacityPercent = offer.BasketCapacityPercent,
            createdAtUnixSeconds = now,
            expiresAtUnixSeconds = expiresAt,
            isRedeemed = false,
            redeemedAtUnixSeconds = 0
        };

        ExchangeChanged?.Invoke();
        return true;
    }

    public bool TryRedeemCode(string rawCode,out InventoryEntryDefinition reward,out string failureReason)
    {
        reward = null;
        failureReason = string.Empty;

        string code = HalloweenPromoCodeService.Normalize(rawCode);

        if (!HalloweenPromoCodeService.IsValidFormat(code))
            return Fail(
                "Промокод должен состоять из 8 символов: A–Z и 0–9.",
                out failureReason);

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory =
            ResourceInventoryManager.Instance;

        if (account == null || inventory == null)
            return Fail("Системы аккаунта или инвентаря ещё не готовы.",out failureReason);

        HalloweenPromoCodeRecord record =
            account.FindHalloweenPromoCode(code);

        if (record == null)
            return Fail("Такой промокод не найден в этом сохранении аккаунта.",out failureReason);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (!HalloweenPromoCodeService.ValidateForRedemption(
                record,
                now,
                out failureReason))
            return false;

        reward = inventory.GetDefinition(record.rewardInventoryId);

        if (reward == null ||
            reward.type != InventoryEntryType.UniqueItem)
        {
            return Fail("Предмет награды не найден или не является уникальным.", out failureReason);
        }

        if (inventory.IsExpired(reward.id))
            return Fail("Срок доступности этой награды истек.",out failureReason);

        if (account.HasUniqueInventoryItem(reward.id))
            return Fail("Этот уникальный предмет уже есть в инвентаре.", out failureReason);

        if (!account.TryRedeemHalloweenPromoCode(code,reward.id,now,out failureReason))
        {
            reward = null;
            return false;
        }

        inventory.RefreshUniqueItemViewAfterExternalGrant(reward.id);

        ExchangeChanged?.Invoke();
        return true;
    }

    private static bool Fail(string message, out string reason)
    {
        reason = message;
        return false;
    }
}