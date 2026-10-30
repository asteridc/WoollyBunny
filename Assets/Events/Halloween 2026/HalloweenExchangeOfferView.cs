
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>UI одного предложения магазина. Расчёты и сохранение сюда не помещаем.</summary>
public sealed class HalloweenExchangeOfferView : MonoBehaviour, IPointerClickHandler
{
    [Header("Offer visuals")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text capacityText;
    [SerializeField] private TMP_Text availabilityText;
    [SerializeField] private TMP_Text rewardAmountText;

    [Header("Purchase button")]
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TMP_Text purchaseButtonText;

    [Header("Purchased visual")]
    [SerializeField] private Image purchasedTintTarget;
    [SerializeField] private Image purchasedCheckmark;
    [SerializeField] private Color purchasedTint = new Color(0.3490196f, 0.7490196f, 0.4509804f, 0.7137255f);

    private readonly List<Graphic> purchasedTintGraphics = new();
    private readonly List<Color> originalTintColors = new();

    private HalloweenExchangeOffer offer;
    private Action<HalloweenExchangeOffer> purchaseCallback;

    private readonly List<Graphic> tintTargets = new List<Graphic>();
    private readonly List<Color> originalColors = new List<Color>();

    private bool tintTargetsCached;

    private void Awake()
    {
        CachePurchasedTintTarget();
    }

    private void OnValidate()
    {
        purchasedTint.a = 1f;
    }

    private void CachePurchasedTintTarget()
    {
        purchasedTintGraphics.Clear();
        originalTintColors.Clear();

        if (purchasedTintTarget == null)
        {
            Debug.LogWarning(
                $"[{nameof(HalloweenExchangeOfferView)}] Не назначен объект Purchased Tint Target.",
                this);

            return;
        }

        Graphic[] graphics = purchasedTintTarget.GetComponentsInChildren<Graphic>(true);

        foreach (Graphic graphic in graphics)
        {
            if (graphic == null)
                continue;

            // Не затемняем кнопку покупки.
            if (purchaseButton != null &&
                (graphic.transform == purchaseButton.transform ||
                 graphic.transform.IsChildOf(purchaseButton.transform)))
                continue;

            // Не затемняем галочку покупки.
            if (purchasedCheckmark != null &&
                (graphic.transform == purchasedCheckmark.transform ||
                 graphic.transform.IsChildOf(purchasedCheckmark.transform)))
                continue;

            purchasedTintGraphics.Add(graphic);
            originalTintColors.Add(graphic.color);
        }
    }

    public void SetPurchasedVisual(bool isPurchased)
    {
        for (int i = 0; i < purchasedTintGraphics.Count; i++)
        {
            Graphic graphic = purchasedTintGraphics[i];

            if (graphic == null)
                continue;

            Color original = originalTintColors[i];

            graphic.color = isPurchased
                ? new Color(
                    purchasedTint.r,
                    purchasedTint.g,
                    purchasedTint.b,
                    original.a)
                : original;
        }

        if (purchasedCheckmark != null)
            purchasedCheckmark.gameObject.SetActive(isPurchased);
    }

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
        Action<HalloweenExchangeOffer> onPurchase,
        bool isPurchased = false)
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

        SetPurchasedVisual(isPurchased);

        if (purchaseButton != null)
        {
            // Кнопка остаётся доступной, чтобы показывать причину отказа.
            purchaseButton.interactable = offerDefinition != null;
            purchaseButton.onClick.AddListener(HandlePurchaseClick);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData == null ||
            eventData.button != PointerEventData.InputButton.Left)
            return;

        if (purchaseButton != null &&
            purchaseButton.gameObject == gameObject)
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