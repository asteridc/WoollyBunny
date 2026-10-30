using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public sealed class HalloweenExchangeUI : MonoBehaviour
{
    [Header("Exchange")]
    [SerializeField] private HalloweenExchangeManager exchangeManager;
    [SerializeField] private Transform offersContent;
    [SerializeField] private TMP_Text currencyBalanceText;
    [SerializeField] private TMP_Text basketCapacityText;
    [SerializeField] private Slider basketCapacitySlider;

    [Header("Purchase confirmation")]
    [SerializeField] private CanvasGroup confirmationGroup;
    [SerializeField] private RectTransform confirmationFrame;
    [SerializeField] private TMP_Text confirmationText;
    [SerializeField] private TMP_Text confirmationErrorText;
    [SerializeField] private Button confirmPurchaseButton;
    [SerializeField] private Button cancelPurchaseButton;

    [Header("Purchase warning (assign the existing warning frame here)")]
    [SerializeField] private CanvasGroup warningGroup;
    [SerializeField] private RectTransform warningFrame;
    [SerializeField] private TMP_Text warningMessageText;
    [SerializeField] private Button closeWarningButton;

    [Header("New promo code")]
    [SerializeField] private CanvasGroup promoResultGroup;
    [SerializeField] private RectTransform promoResultFrame;
    [SerializeField] private TMP_Text promoCodeText;
    [SerializeField] private TMP_Text promoExpiryText;
    [SerializeField] private TMP_Text copyFeedbackText;
    [SerializeField] private Button copyPromoCodeButton;
    [SerializeField] private Button closePromoResultButton;

    [Header("Redeem code")]
    [SerializeField] private TMP_InputField promoCodeInput;
    [SerializeField] private Button redeemPromoCodeButton;
    [SerializeField] private TMP_Text redeemFeedbackText;

    [Header("Promo history")]
    [SerializeField] private Button openHistoryButton;
    [SerializeField] private CanvasGroup historyGroup;
    [SerializeField] private RectTransform historyFrame;
    [SerializeField] private Transform historyContent;
    [SerializeField] private HalloweenPromoCodeHistoryItemView historyItemPrefab;
    [SerializeField] private TMP_Text historyEmptyText;
    [SerializeField] private Button closeHistoryButton;

    [Header("Animation")]
    [SerializeField, Min(0.01f)]
    private float panelAnimationDuration = 0.22f;

    [SerializeField, Range(0.8f, 0.99f)]
    private float hiddenScale = 0.95f;

    private HalloweenExchangeOffer selectedOffer;
    private string latestGeneratedCode;

    private readonly List<HalloweenExchangeOfferView> offerViews =
        new List<HalloweenExchangeOfferView>();

    private void Awake()
    {
        CacheOfferViews();

        HidePanelInstant(confirmationGroup, confirmationFrame);
        HidePanelInstant(warningGroup, warningFrame);
        HidePanelInstant(promoResultGroup, promoResultFrame);

        if (confirmationText == null)
        {
            Debug.LogWarning(
                "HalloweenExchangeUI: не назначен Confirmation Text. " +
                "Перетащи сюда TMP_Text, в котором должен меняться текст подтверждения.",
                this);
        }

        if (warningGroup == null || warningFrame == null || warningMessageText == null)
        {
            Debug.LogWarning(
                "HalloweenExchangeUI: назначь Warning Group, Warning Frame и Warning Message Text, " +
                "чтобы показывать причины недоступной покупки.",
                this);
        }
        HidePanelInstant(historyGroup, historyFrame);
    }

    private void CacheOfferViews()
    {
        offerViews.Clear();

        if (offersContent == null)
        {
            Debug.LogWarning(
                "HalloweenExchangeUI: assign Offers Content in the Inspector.",
                this);
            return;
        }

        // Use the offer views already placed in the scene.
        // This component no longer creates or destroys offer-card objects.
        offerViews.AddRange(
            offersContent.GetComponentsInChildren<HalloweenExchangeOfferView>(true));
    }

    private void OnEnable()
    {
        if (confirmPurchaseButton != null)
            confirmPurchaseButton.onClick.AddListener(
                ConfirmSelectedPurchase);

        if (cancelPurchaseButton != null)
            cancelPurchaseButton.onClick.AddListener(CancelPurchase);

        if (closeWarningButton != null)
            closeWarningButton.onClick.AddListener(CloseWarning);

        if (copyPromoCodeButton != null)
            copyPromoCodeButton.onClick.AddListener(CopyLatestCode);

        if (closePromoResultButton != null)
            closePromoResultButton.onClick.AddListener(ClosePromoResult);

        if (redeemPromoCodeButton != null)
            redeemPromoCodeButton.onClick.AddListener(RedeemEnteredCode);

        if (openHistoryButton != null)
            openHistoryButton.onClick.AddListener(OpenHistory);

        if (closeHistoryButton != null)
            closeHistoryButton.onClick.AddListener(CloseHistory);

        if (promoCodeInput != null)
            promoCodeInput.onSubmit.AddListener(HandlePromoInputSubmit);

        if (exchangeManager != null)
            exchangeManager.ExchangeChanged += RefreshAll;

        RefreshAll();
    }

    private void OnDisable()
    {
        if (confirmPurchaseButton != null)
            confirmPurchaseButton.onClick.RemoveListener(
                ConfirmSelectedPurchase);

        if (cancelPurchaseButton != null)
            cancelPurchaseButton.onClick.RemoveListener(CancelPurchase);

        if (closeWarningButton != null)
            closeWarningButton.onClick.RemoveListener(CloseWarning);

        if (copyPromoCodeButton != null)
            copyPromoCodeButton.onClick.RemoveListener(CopyLatestCode);

        if (closePromoResultButton != null)
            closePromoResultButton.onClick.RemoveListener(ClosePromoResult);

        if (redeemPromoCodeButton != null)
            redeemPromoCodeButton.onClick.RemoveListener(RedeemEnteredCode);

        if (openHistoryButton != null)
            openHistoryButton.onClick.RemoveListener(OpenHistory);

        if (closeHistoryButton != null)
            closeHistoryButton.onClick.RemoveListener(CloseHistory);

        if (promoCodeInput != null)
            promoCodeInput.onSubmit.RemoveListener(HandlePromoInputSubmit);

        if (exchangeManager != null)
            exchangeManager.ExchangeChanged -= RefreshAll;

        KillPanelTweens(confirmationGroup, confirmationFrame);
        KillPanelTweens(warningGroup, warningFrame);
        KillPanelTweens(promoResultGroup, promoResultFrame);
        KillPanelTweens(historyGroup, historyFrame);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (IsPanelVisible(confirmationGroup))
            CancelPurchase();
        else if (IsPanelVisible(warningGroup))
            CloseWarning();
        else if (IsPanelVisible(promoResultGroup))
            ClosePromoResult();
        else if (IsPanelVisible(historyGroup))
            CloseHistory();
    }

    public void RefreshAll()
    {
        RefreshBalanceAndBasket();
        RefreshOfferList();
    }

    private void RefreshBalanceAndBasket()
    {
        if (exchangeManager == null)
            return;

        bool english =
            LanguageManager.CurrentLanguage == Language.English;

        int balance = exchangeManager.GetCurrencyBalance();
        int usedCapacity = exchangeManager.GetBasketCapacityUsed();

        if (currencyBalanceText != null)
            currencyBalanceText.text = english
                ? $"{balance}"
                : $"{balance}";

        if (basketCapacityText != null)
            basketCapacityText.text = english
                ? $"{usedCapacity}%"
                : $"{usedCapacity}%";

        if (basketCapacitySlider != null)
        {
            basketCapacitySlider.minValue = 0;
            basketCapacitySlider.maxValue = 100;
            basketCapacitySlider.value =
                Mathf.Clamp(usedCapacity, 0, 100);
        }
    }

    private void RefreshOfferList()
    {
        if (exchangeManager == null || offersContent == null)
            return;

        IReadOnlyList<HalloweenExchangeOffer> offers =
            exchangeManager.Offers;

        int viewIndex = 0;

        for (int i = 0; i < offers.Count; i++)
        {
            HalloweenExchangeOffer offer = offers[i];

            if (offer == null)
                continue;

            if (viewIndex >= offerViews.Count)
            {
                Debug.LogWarning(
                    $"HalloweenExchangeUI: there are {offers.Count} offers, " +
                    $"but only {offerViews.Count} pre-placed offer views. " +
                    "Add a HalloweenExchangeOfferView under Offers Content for each offer.",
                    this);
                break;
            }

            HalloweenExchangeOfferView view = offerViews[viewIndex++];

            if (view == null)
                continue;

            InventoryEntryDefinition reward =
                ResourceInventoryManager.Instance != null
                    ? ResourceInventoryManager.Instance.GetDefinition(
                        offer.RewardInventoryId)
                    : null;

            bool canPurchase =
                exchangeManager.CanPurchase(offer, out string reason);

            view.gameObject.SetActive(true);
            view.Bind(
                offer,
                reward,
                canPurchase,
                reason,
                OnOfferSelected);
        }

        // Hide any pre-placed views that do not have a corresponding offer.
        for (int i = viewIndex; i < offerViews.Count; i++)
        {
            if (offerViews[i] != null)
                offerViews[i].gameObject.SetActive(false);
        }
    }

    private void OnOfferSelected(HalloweenExchangeOffer offer)
    {
        if (exchangeManager == null || offer == null)
            return;

        if (!exchangeManager.CanPurchase(offer, out string reason))
        {
            selectedOffer = null;
            ShowWarning(reason);
            RefreshAll();
            return;
        }

        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        InventoryEntryDefinition reward = inventory != null
            ? inventory.GetDefinition(offer.RewardInventoryId)
            : null;

        if (reward == null)
        {
            selectedOffer = null;
            ShowWarning(LanguageManager.CurrentLanguage == Language.English
                ? "The reward is missing from the inventory catalog."
                : "Предмет награды не найден в каталоге инвентаря.");
            return;
        }

        selectedOffer = offer;
        bool english = LanguageManager.CurrentLanguage == Language.English;
        string rewardName = english ? reward.englishName : reward.russianName;
        string rarityName = HalloweenRarityColorUtility.GetLocalizedName(reward.rarity, english);
        string color = HalloweenRarityColorUtility.ToHex(reward.rarity);
        string highlightedReward = $"<color=#{color}>{rewardName} ({rarityName})</color>";

        if (confirmationText != null)
        {
            confirmationText.text = english
                ? $"Are you sure you want to purchase {highlightedReward} " +
                  $"for {offer.CandyPrice} Scandy? It will use " +
                  $"{offer.BasketCapacityPercent}% of your basket. " +
                  $"The promo code expires in {exchangeManager.PromoCodeLifetimeDays} days."
                : $"Ты действительно хочешь приобрести {highlightedReward} " +
                  $"за {offer.CandyPrice} Конфетыкв? Предмет займёт " +
                  $"{offer.BasketCapacityPercent}% корзины. Промокод истечёт через " +
                  $"{exchangeManager.PromoCodeLifetimeDays} дн.";
        }
        else
        {
            Debug.LogError("HalloweenExchangeUI: Confirmation Text не назначен в Inspector.", this);
        }

        if (confirmationErrorText != null)
            confirmationErrorText.text = string.Empty;

        HidePanelInstant(warningGroup, warningFrame);
        ShowPanel(confirmationGroup, confirmationFrame, confirmPurchaseButton);
    }

    private void ShowWarning(string message)
    {
        if (warningMessageText != null)
            warningMessageText.text = message;

        if (warningGroup == null || warningFrame == null)
        {
            Debug.LogWarning(
                "HalloweenExchangeUI: фрейм предупреждения не назначен. Причина: " + message,
                this);
            return;
        }

        HidePanelInstant(confirmationGroup, confirmationFrame);
        ShowPanel(warningGroup, warningFrame, closeWarningButton);
    }

    private void CloseWarning()
    {
        HidePanel(warningGroup, warningFrame, null);
    }

    private void ConfirmSelectedPurchase()
    {
        if (exchangeManager == null || selectedOffer == null)
            return;

        if (!exchangeManager.TryPurchaseOffer(
                selectedOffer,
                out HalloweenPromoCodeRecord record,
                out string reason))
        {
            selectedOffer = null;
            HidePanelInstant(confirmationGroup, confirmationFrame);
            ShowWarning(reason);
            RefreshAll();
            return;
        }

        latestGeneratedCode = record.code;

        HidePanel(
            confirmationGroup,
            confirmationFrame,
            () =>
            {
                selectedOffer = null;
                ShowPromoResult(record);
            });

        RefreshAll();
    }

    private void CancelPurchase()
    {
        HidePanel(
            confirmationGroup,
            confirmationFrame,
            () => selectedOffer = null);
    }

    private void ShowPromoResult(HalloweenPromoCodeRecord record)
    {
        if (record == null)
            return;

        latestGeneratedCode = record.code;

        bool english =
            LanguageManager.CurrentLanguage == Language.English;

        if (promoCodeText != null)
            promoCodeText.text = record.code;

        if (copyFeedbackText != null)
            copyFeedbackText.text = string.Empty;

        if (promoExpiryText != null)
        {
            DateTimeOffset expiry =
                DateTimeOffset
                    .FromUnixTimeSeconds(record.expiresAtUnixSeconds)
                    .ToLocalTime();

            promoExpiryText.text = english
                ? $"Enter this code in the event screen before " +
                  $"{expiry:yyyy-MM-dd HH:mm}."
                : $"Введи этот код в поле погашения до " +
                  $"{expiry:dd.MM.yyyy HH:mm}.";
        }

        ShowPanel(
            promoResultGroup,
            promoResultFrame,
            closePromoResultButton);
    }

    private void CopyLatestCode()
    {
        if (string.IsNullOrEmpty(latestGeneratedCode))
            return;

        GUIUtility.systemCopyBuffer = latestGeneratedCode;

        if (copyFeedbackText != null)
        {
            copyFeedbackText.text =
                LanguageManager.CurrentLanguage == Language.English
                    ? "Copied to clipboard"
                    : "Промокод скопирован";
        }
    }

    private void ClosePromoResult()
    {
        HidePanel(promoResultGroup, promoResultFrame, null);
    }

    private void HandlePromoInputSubmit(string ignored)
    {
        RedeemEnteredCode();
    }

    private void RedeemEnteredCode()
    {
        if (exchangeManager == null || promoCodeInput == null)
            return;

        string enteredCode = promoCodeInput.text;

        if (string.IsNullOrWhiteSpace(enteredCode))
            return;

        if (!exchangeManager.TryRedeemCode(
                enteredCode,
                out InventoryEntryDefinition reward,
                out string reason))
        {
            if (redeemFeedbackText != null)
                redeemFeedbackText.text = reason;

            return;
        }

        bool english =
            LanguageManager.CurrentLanguage == Language.English;

        string rewardName = english
            ? reward.englishName
            : reward.russianName;

        string color =
            HalloweenRarityColorUtility.ToHex(reward.rarity);

        if (redeemFeedbackText != null)
        {
            redeemFeedbackText.text = english
                ? $"Code accepted! You received " +
                  $"<color=#{color}>{rewardName}</color>."
                : $"Промокод принят! Получен предмет " +
                  $"<color=#{color}>{rewardName}</color>.";
        }

        promoCodeInput.text = string.Empty;

        RefreshAll();
        RefreshHistoryIfOpen();
    }

    public void OpenHistory()
    {
        RefreshHistoryItems();
        ShowPanel(historyGroup, historyFrame, closeHistoryButton);
    }

    public void CloseHistory()
    {
        HidePanel(historyGroup, historyFrame, null);
    }

    private void RefreshHistoryIfOpen()
    {
        if (IsPanelVisible(historyGroup))
            RefreshHistoryItems();
    }

    private void RefreshHistoryItems()
    {
        if (exchangeManager == null ||
            historyContent == null ||
            historyItemPrefab == null)
            return;

        HalloweenPromoCodeHistoryItemView[] oldItems =
            historyContent.GetComponentsInChildren<
                HalloweenPromoCodeHistoryItemView>(true);

        foreach (HalloweenPromoCodeHistoryItemView oldItem in oldItems)
            Destroy(oldItem.gameObject);

        List<HalloweenPromoCodeRecord> history =
            exchangeManager.GetPromoHistory();

        if (historyEmptyText != null)
        {
            bool hasHistory = history.Count > 0;
            historyEmptyText.gameObject.SetActive(!hasHistory);

            if (!hasHistory)
            {
                historyEmptyText.text =
                    LanguageManager.CurrentLanguage == Language.English
                        ? "No promo codes yet"
                        : "Промокодов пока нет";
            }
        }

        foreach (HalloweenPromoCodeRecord record in history)
        {
            HalloweenPromoCodeHistoryItemView item =
                Instantiate(historyItemPrefab, historyContent);

            item.Bind(record);
        }
    }

    private void ShowPanel(
        CanvasGroup group,
        RectTransform frame,
        Button focusButton)
    {
        if (group == null || frame == null)
        {
            Debug.LogWarning(
                "HalloweenExchangeUI: назначь CanvasGroup и RectTransform панели в Inspector.",
                this);
            return;
        }

        KillPanelTweens(group, frame);

        group.gameObject.SetActive(true);
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = true;

        frame.localScale = Vector3.one * hiddenScale;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        sequence.Join(
            group.DOFade(1f, panelAnimationDuration));

        sequence.Join(
            frame.DOScale(1f, panelAnimationDuration)
                .SetEase(Ease.OutCubic));

        sequence.OnComplete(() =>
        {
            if (group != null)
                group.interactable = true;

            if (focusButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(
                    focusButton.gameObject);
            }
        });
    }

    private void HidePanel(
        CanvasGroup group,
        RectTransform frame,
        Action onClosed)
    {
        if (group == null ||
            frame == null ||
            !group.gameObject.activeSelf)
        {
            onClosed?.Invoke();
            return;
        }

        KillPanelTweens(group, frame);

        group.interactable = false;
        group.blocksRaycasts = true;

        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        sequence.Join(
            group.DOFade(0f, panelAnimationDuration));

        sequence.Join(
            frame.DOScale(hiddenScale, panelAnimationDuration)
                .SetEase(Ease.InCubic));

        sequence.OnComplete(() =>
        {
            if (group != null)
            {
                group.blocksRaycasts = false;
                group.gameObject.SetActive(false);
            }

            onClosed?.Invoke();
        });
    }

    private void HidePanelInstant(
        CanvasGroup group,
        RectTransform frame)
    {
        if (group == null)
            return;

        KillPanelTweens(group, frame);

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        group.gameObject.SetActive(false);

        if (frame != null)
            frame.localScale = Vector3.one * hiddenScale;
    }

    private static void KillPanelTweens(
        CanvasGroup group,
        RectTransform frame)
    {
        if (group != null)
            group.DOKill();

        if (frame != null)
            frame.DOKill();
    }

    private static bool IsPanelVisible(CanvasGroup group)
    {
        return group != null && group.gameObject.activeSelf;
    }
}