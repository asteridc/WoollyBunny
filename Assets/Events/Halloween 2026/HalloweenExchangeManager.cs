using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Бизнес-логика Хеллоуинского обмена: проверяет покупки, генерирует промокоды
/// и погашает код, добавляя уникальную награду в существующий аккаунтный инвентарь.
/// UI к этому классу не привязан.
/// </summary>
public sealed class HalloweenExchangeManager : MonoBehaviour
{
    [Header("Offers")]
    [SerializeField] private HalloweenExchangeOffer[] offers = Array.Empty<HalloweenExchangeOffer>();

    [Header("Currency")]
    [SerializeField] private string currencyId = "Конфетыквы";

    [Header("Promo code")]
    [SerializeField, Min(1)] private int promoCodeLifetimeDays = 7;

    [Header("Debug tools (Play Mode only in custom Inspector)")]
    [SerializeField] private string debugResourceId = "Конфетыквы";
    [SerializeField, Min(0)] private int debugResourceAmount = 5000;
    [SerializeField] private bool debugRemoveOfferRewards = true;

    public event Action ExchangeChanged;

    public IReadOnlyList<HalloweenExchangeOffer> Offers => offers;
    public int PromoCodeLifetimeDays => Mathf.Max(1, promoCodeLifetimeDays);
    public string DebugResourceId => debugResourceId;
    public int DebugResourceAmount => debugResourceAmount;
    public bool DebugRemoveOfferRewards => debugRemoveOfferRewards;

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
            if (offer != null && string.Equals(
                    offer.OfferId?.Trim(), offerId.Trim(), StringComparison.OrdinalIgnoreCase))
                return offer;
        }

        return null;
    }

    public bool CanPurchase(HalloweenExchangeOffer offer, out string reason)
    {
        reason = string.Empty;

        if (offer == null || !offer.Available)
            return Fail(Localized(
                "Это предложение сейчас недоступно.",
                "This offer is currently unavailable."), out reason);

        if (string.IsNullOrWhiteSpace(offer.OfferId) ||
            string.IsNullOrWhiteSpace(offer.RewardInventoryId) ||
            offer.CandyPrice <= 0 ||
            offer.BasketCapacityPercent < 1 ||
            offer.BasketCapacityPercent > 100)
        {
            return Fail(Localized(
                "У предложения некорректно настроены ID, цена или вместимость.",
                "This offer has an invalid ID, price, or basket capacity."), out reason);
        }

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        if (account == null || inventory == null)
            return Fail(Localized(
                "Системы аккаунта или инвентаря ещё не готовы.",
                "Account or inventory systems are not ready yet."), out reason);

        InventoryEntryDefinition reward =
    inventory.GetDefinition(offer.RewardInventoryId);

        if (reward == null)
        {
            return Fail(Localized(
                "Награда отсутствует в каталоге ResourceInventoryManager.",
                "The reward is missing from the ResourceInventoryManager catalog."),
                out reason);
        }

        // Допустимы уникальные предметы и стакабельные ресурсы.
        if (reward.type != InventoryEntryType.UniqueItem &&
            reward.type != InventoryEntryType.StackableResource)
        {
            return Fail(Localized(
                "Обмен поддерживает только UniqueItem и StackableResource.",
                "The exchange supports UniqueItem and StackableResource rewards only."),
                out reason);
        }

        if (inventory.IsExpired(reward.id))
        {
            return Fail(Localized(
                "Срок действия этой награды закончился.",
                "This reward has expired."),
                out reason);
        }

        // Каждый конкретный оффер можно купить только один раз.
        if (account.HasHalloweenPromoForOffer(offer.OfferId))
        {
            return Fail(Localized(
                "Это предложение уже было куплено.",
                "This offer has already been purchased."),
                out reason);
        }

        // Проверки владения применяются только к UniqueItem.
        if (reward.type == InventoryEntryType.UniqueItem)
        {
            if (account.HasUniqueInventoryItem(reward.id))
            {
                return Fail(Localized(
                    "Этот уникальный предмет у тебя уже есть.",
                    "You already own this unique item."),
                    out reason);
            }

            if (account.HasHalloweenPromoForReward(reward.id))
            {
                return Fail(Localized(
                    "Ты уже покупал промокод на этот предмет. Повторная покупка недоступна.",
                    "A promo code for this unique item has already been purchased."),
                    out reason);
            }
        }

        // Accumulate both resource-related failures so the warning can tell the player
        // about insufficient candy and insufficient basket capacity at the same time.
        List<string> failures = new List<string>();

        if (GetBasketCapacityUsed() + offer.BasketCapacityPercent > 100)
        {
            int remaining = Mathf.Max(0, 100 - GetBasketCapacityUsed());
            failures.Add(Localized(
                $"В корзине не хватает места. Свободно: {remaining}%, требуется: {offer.BasketCapacityPercent}%.",
                $"Not enough basket capacity. Available: {remaining}%, required: {offer.BasketCapacityPercent}%."));
        }

        if (GetCurrencyBalance() < offer.CandyPrice)
        {
            int missing = Mathf.Max(0, offer.CandyPrice - GetCurrencyBalance());
            failures.Add(Localized(
                $"Недостаточно Конфетыкв. Не хватает: {missing}.",
                $"Not enough Scandy. Missing: {missing}."));
        }

        if (failures.Count > 0)
        {
            reason = string.Join("\n\n", failures);
            return false;
        }

        return true;
    }

    private static string Localized(string russian, string english)
    {
        return LanguageManager.CurrentLanguage == Language.English
            ? english
            : russian;
    }

    /// <summary>
    /// Покупка только создаёт промокод и фиксирует цену/вместимость.
    /// Сам уникальный предмет будет добавлен после успешного ввода кода.
    /// </summary>
    // HALLOWEEN_HISTORY_QUANTITY_FIX_PURCHASE
    /// <summary>
    /// Покупка создаёт промокод и фиксирует стоимость, вместимость и количество награды.
    /// Награда выдаётся только после успешного погашения промокода.
    /// </summary>
    public bool TryPurchaseOffer(
        HalloweenExchangeOffer offer,
        out HalloweenPromoCodeRecord createdRecord,
        out string failureReason)
    {
        createdRecord = null;
        if (!CanPurchase(offer, out failureReason))
            return false;

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        InventoryEntryDefinition reward = inventory != null
            ? inventory.GetDefinition(offer.RewardInventoryId)
            : null;

        if (reward == null)
            return Fail(Localized(
                "Награда отсутствует в каталоге инвентаря.",
                "The reward is missing from the inventory catalog."), out failureReason);

        int rewardAmount = reward.type == InventoryEntryType.UniqueItem
            ? 1
            : offer.RewardAmount;

        if (rewardAmount < 1)
            return Fail(Localized(
                "Количество награды должно быть не меньше 1.",
                "Reward amount must be at least 1."), out failureReason);

        if (!HalloweenPromoCodeService.TryGenerateUniqueCode(account, out string code))
            return Fail(Localized(
                "Не удалось создать уникальный промокод. Попробуй ещё раз.",
                "Could not generate a unique promo code. Please try again."), out failureReason);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!HalloweenPromoCodeService.TryCalculateExpiry(now, PromoCodeLifetimeDays, out long expiresAt))
            return Fail(Localized(
                "Не удалось рассчитать срок действия промокода.",
                "Could not calculate the promo-code expiry."), out failureReason);

        bool purchased = account.TryPurchaseHalloweenOffer(
            currencyId,
            offer.OfferId,
            offer.RewardInventoryId,
            offer.CandyPrice,
            offer.BasketCapacityPercent,
            rewardAmount,
            code,
            now,
            expiresAt,
            out failureReason);

        if (!purchased)
            return false;

        inventory.NotifyAccountAmountChanged(currencyId);

        createdRecord = new HalloweenPromoCodeRecord
        {
            code = code,
            offerId = offer.OfferId,
            rewardInventoryId = offer.RewardInventoryId,
            rewardAmount = rewardAmount,
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
    /// <summary>
    /// Погашение проверяется по локальной истории текущего аккаунта.
    /// Для общих между устройствами кодов требуется серверная база данных.
    /// </summary>
    // HALLOWEEN_HISTORY_QUANTITY_FIX_REDEEM
    /// <summary>Погашает промокод и выдаёт сохранённое в нём количество награды.</summary>
    public bool TryRedeemCode(
        string rawCode,
        out InventoryEntryDefinition reward,
        out string failureReason)
    {
        return TryRedeemCode(rawCode, out reward, out _, out failureReason);
    }

    public bool TryRedeemCode(
        string rawCode,
        out InventoryEntryDefinition reward,
        out int rewardAmount,
        out string failureReason)
    {
        reward = null;
        rewardAmount = 0;
        failureReason = string.Empty;

        string code = HalloweenPromoCodeService.Normalize(rawCode);
        if (!HalloweenPromoCodeService.IsValidFormat(code))
            return Fail(Localized(
                "Промокод должен состоять из 8 символов: A–Z и 0–9.",
                "The promo code must contain 8 characters: A–Z and 0–9."), out failureReason);

        AccountManager account = AccountManager.Instance;
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        if (account == null || inventory == null)
            return Fail(Localized(
                "Системы аккаунта или инвентаря ещё не готовы.",
                "Account or inventory systems are not ready yet."), out failureReason);

        HalloweenPromoCodeRecord record = account.FindHalloweenPromoCode(code);
        if (record == null)
            return Fail(Localized(
                "Такой промокод не найден в этом сохранении аккаунта.",
                "This promo code was not found in this account save."), out failureReason);

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!HalloweenPromoCodeService.ValidateForRedemption(record, now, out failureReason))
            return false;

        reward = inventory.GetDefinition(record.rewardInventoryId);
        if (reward == null ||
            (reward.type != InventoryEntryType.UniqueItem &&
             reward.type != InventoryEntryType.StackableResource))
        {
            reward = null;
            return Fail(Localized(
                "Награда не найдена или имеет неподдерживаемый тип.",
                "The reward is missing or has an unsupported type."), out failureReason);
        }

        if (inventory.IsExpired(reward.id))
        {
            reward = null;
            return Fail(Localized(
                "Срок доступности этой награды закончился.",
                "This reward has expired."), out failureReason);
        }

        if (reward.type == InventoryEntryType.UniqueItem)
        {
            if (account.HasUniqueInventoryItem(reward.id))
            {
                reward = null;
                return Fail(Localized(
                    "Этот уникальный предмет уже есть в инвентаре.",
                    "You already own this unique item."), out failureReason);
            }
            rewardAmount = 1;
        }
        else
        {
            rewardAmount = record.rewardAmount;
            if (rewardAmount < 1)
            {
                reward = null;
                return Fail(Localized(
                    "В промокоде не указано корректное количество награды.",
                    "The promo code does not contain a valid reward quantity."), out failureReason);
            }
        }

        if (!account.TryRedeemHalloweenPromoCode(
                code, reward.id, reward.type, now, out failureReason))
        {
            reward = null;
            rewardAmount = 0;
            return false;
        }

        if (reward.type == InventoryEntryType.UniqueItem)
            inventory.RefreshUniqueItemViewAfterExternalGrant(reward.id);
        else
            inventory.NotifyAccountAmountChanged(reward.id);

        ExchangeChanged?.Invoke();
        return true;
    }
    /// <summary>Добавляет указанное количество обычного ресурса для тестирования.</summary>
    public bool DebugAddResource()
    {
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        if (inventory == null || string.IsNullOrWhiteSpace(debugResourceId) || debugResourceAmount <= 0)
        {
            Debug.LogWarning("HalloweenExchangeManager: укажи ID ресурса и количество больше нуля.", this);
            return false;
        }

        InventoryEntryDefinition definition = inventory.GetDefinition(debugResourceId);
        if (definition == null || definition.type == InventoryEntryType.UniqueItem)
        {
            Debug.LogWarning("HalloweenExchangeManager: ресурс не найден или является UniqueItem.", this);
            return false;
        }

        bool result = inventory.Add(debugResourceId, debugResourceAmount);
        if (result)
        {
            Debug.Log($"[Halloween Exchange Debug] Добавлено {debugResourceAmount} ресурса '{debugResourceId}'.", this);
            ExchangeChanged?.Invoke();
        }
        return result;
    }

    /// <summary>Устанавливает точный баланс указанного обычного ресурса для тестирования.</summary>
    public bool DebugSetResourceAmount()
    {
        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        if (inventory == null || string.IsNullOrWhiteSpace(debugResourceId) || debugResourceAmount < 0)
        {
            Debug.LogWarning("HalloweenExchangeManager: укажи ID ресурса и неотрицательное количество.", this);
            return false;
        }

        InventoryEntryDefinition definition = inventory.GetDefinition(debugResourceId);
        if (definition == null || definition.type == InventoryEntryType.UniqueItem)
        {
            Debug.LogWarning("HalloweenExchangeManager: ресурс не найден или является UniqueItem.", this);
            return false;
        }

        bool result = inventory.SetAmount(debugResourceId, debugResourceAmount);
        if (result)
        {
            Debug.Log($"[Halloween Exchange Debug] Баланс ресурса '{debugResourceId}' установлен на {debugResourceAmount}.", this);
            ExchangeChanged?.Invoke();
        }
        return result;
    }

    /// <summary>
    /// Сбрасывает тестовое состояние предложений: корзину и промокоды;
    /// при включённой настройке также удаляет награды из инвентаря.
    /// Баланс валюты не меняет.
    /// </summary>
    public bool DebugResetShopState()
    {
        AccountManager account = AccountManager.Instance;
        if (account == null)
        {
            Debug.LogError("HalloweenExchangeManager: AccountManager не найден — сброс невозможен.", this);
            return false;
        }

        List<string> offerIds = new List<string>();
        List<string> rewardIds = new List<string>();
        if (offers != null)
        {
            foreach (HalloweenExchangeOffer offer in offers)
            {
                if (offer == null)
                    continue;
                if (!string.IsNullOrWhiteSpace(offer.OfferId))
                    offerIds.Add(offer.OfferId);
                if (!string.IsNullOrWhiteSpace(offer.RewardInventoryId))
                    rewardIds.Add(offer.RewardInventoryId);
            }
        }

        if (!account.ResetHalloweenExchangeState(offerIds, rewardIds, debugRemoveOfferRewards))
        {
            Debug.LogError("HalloweenExchangeManager: не удалось сохранить сброс состояния магазина.", this);
            return false;
        }

        if (debugRemoveOfferRewards && ResourceInventoryManager.Instance != null)
            ResourceInventoryManager.Instance.RefreshOwnedUniqueItemViewsAfterDebugReset();

        ExchangeChanged?.Invoke();
        Debug.Log(
            "[Halloween Exchange Debug] Состояние магазина сброшено: корзина очищена, промокоды предложений удалены" +
            (debugRemoveOfferRewards ? ", награды из пула удалены из инвентаря." : ". Предметы оставлены в инвентаре."),
            this);
        return true;
    }

    private static bool Fail(string message, out string reason)
    {
        reason = message;
        return false;
    }
}
