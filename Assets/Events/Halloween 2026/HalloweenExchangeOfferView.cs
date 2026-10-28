using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>UI одного предложения магазина. Расчёты и сохранение сюда не помещаем.</summary>
public sealed class HalloweenExchangeOfferView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text availabilityText;
    [SerializeField] private TMP_Text rewardAmountText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseButtonText;

    private HalloweenExchangeOffer offer;
    private Action<HalloweenExchangeOffer> purchaseCallback;

    private void OnDestroy()
    {
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(HandlePurchaseClick);
    }

    public void Bind(
        HalloweenExchangeOffer offerDefinition,
        InventoryEntryDefinition reward,
        bool canPurchase,
        string unavailableReason,
        Action<HalloweenExchangeOffer> onPurchase)
    {
        if (purchaseButton != null)
            purchaseButton.onClick.RemoveListener(HandlePurchaseClick);

        offer = offerDefinition;
        purchaseCallback = onPurchase;

        bool english = LanguageManager.CurrentLanguage == Language.English;

        if (reward != null)
        {
            if (iconImage != null)
            {
                iconImage.sprite = reward.icon;
                iconImage.enabled = reward.icon != null;
            }

            string rewardName = english ? reward.englishName : reward.russianName;
            string color = HalloweenRarityColorUtility.ToHex(reward.rarity);

            if (nameText != null)
                nameText.text = $"<color=#{color}>{rewardName}</color>";

            if (rarityText != null)
            {
                rarityText.text =
                    $"<color=#{color}>{HalloweenRarityColorUtility.GetLocalizedName(reward.rarity, english)}</color>";
            }
        }
        else
        {
            if (nameText != null)
                nameText.text = english ? "Reward not configured" : "Награда не настроена";

            if (rarityText != null)
                rarityText.text = string.Empty;

            if (iconImage != null)
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }

        if (priceText != null && offerDefinition != null)
            priceText.text = $"/ {offerDefinition.CandyPrice}";

        if (capacityText != null && offerDefinition != null)
            capacityText.text = $"{offerDefinition.BasketCapacityPercent}%";

        if (rewardAmountText != null)
        {
            bool isStackable = reward != null &&
                               reward.type == InventoryEntryType.StackableResource &&
                               offerDefinition != null;
            rewardAmountText.text = isStackable
                ? $"{offerDefinition.RewardAmount}"
                : string.Empty;
        }

        if (availabilityText != null)
            availabilityText.text = canPurchase ? string.Empty : unavailableReason;

        if (purchaseButtonText != null)
            purchaseButtonText.text = english ? "Choose" : "Выбрать";

        if (purchaseButton != null)
        {
            // The button must remain clickable even when CanPurchase is false,
            // so the warning panel can explain the reason.
            purchaseButton.interactable = offerDefinition != null;
            purchaseButton.onClick.AddListener(HandlePurchaseClick);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null || eventData.button != PointerEventData.InputButton.Left)
            return;

        // If the Button is on this same GameObject, its own onClick listener handles it.
        if (purchaseButton != null && purchaseButton.gameObject == gameObject)
            return;

        HandlePurchaseClick();
    }

    private void HandlePurchaseClick()
    {
        if (offer == null)
            return;

        Debug.Log($"[Halloween Exchange] Выбран оффер: {offer.OfferId}", this);
        purchaseCallback?.Invoke(offer);
    }
}

public static class HalloweenRarityColorUtility
{
    public static string ToHex(ResourceRarity rarity)
    {
        switch (rarity)
        {
            case ResourceRarity.Rare: return "63C7FF";
            case ResourceRarity.Epic: return "C77DFF";
            case ResourceRarity.Relic: return "FFD54A";
            case ResourceRarity.Unique: return "FF6868";
            default: return "EEEEEE";
        }
    }

    public static string GetLocalizedName(ResourceRarity rarity, bool english)
    {
        switch (rarity)
        {
            case ResourceRarity.Rare: return english ? "Rare" : "Редкий";
            case ResourceRarity.Epic: return english ? "Epic" : "Эпический";
            case ResourceRarity.Relic: return english ? "Relic" : "Реликтовый";
            case ResourceRarity.Unique: return english ? "Unique" : "Уникальный";
            default: return english ? "Common" : "Обычный";
        }
    }
}
