using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ResourceAmountDisplay : MonoBehaviour
{
    [SerializeField] private string entryId;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private Image iconImage;

    private CanvasGroup canvasGroup;

    public string EntryId => entryId;

    private void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (ResourceInventoryManager.Instance != null)
            Bind(ResourceInventoryManager.Instance);

        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        if (ResourceInventoryManager.Instance != null)
        {
            ResourceInventoryManager.Instance.AmountChanged -= OnAmountChanged;
            ResourceInventoryManager.Instance.InventoryChanged -= OnInventoryChanged;
        }
    }

    public void Bind(ResourceInventoryManager manager)
    {
        if (manager == null)
            return;

        manager.AmountChanged -= OnAmountChanged;
        manager.AmountChanged += OnAmountChanged;
        manager.InventoryChanged -= OnInventoryChanged;
        manager.InventoryChanged += OnInventoryChanged;
        Refresh(manager.GetDefinition(entryId), manager.GetAmount(entryId), manager.IsExpired(entryId));
    }

    public void Refresh(InventoryEntryDefinition definition, int amount, bool expired)
    {
        if (definition == null)
        {
            if (amountText != null)
                amountText.text = string.Empty;
            if (nameText != null)
                nameText.text = string.Empty;
            SetVisible(false);
            return;
        }

        if (amountText != null)
        {
            amountText.text = definition.type == InventoryEntryType.UniqueItem
                ? string.Empty
                : amount.ToString();
        }

        if (nameText != null)
        {
            nameText.text = LanguageManager.CurrentLanguage == Language.English
                ? definition.englishName
                : definition.russianName;
        }

        if (rarityText != null)
        {
            bool english = LanguageManager.CurrentLanguage == Language.English;
            rarityText.text = definition.type == InventoryEntryType.Currency
                ? string.Empty
                : ResourceItemView.GetLocalizedRarityText(definition.rarity, english);
        }

        if (iconImage != null)
            iconImage.sprite = definition.icon;

        SetVisible(definition.type != InventoryEntryType.UniqueItem || amount > 0);
        if (canvasGroup != null)
            canvasGroup.alpha = expired ? 0.5f : 1f;
    }

    private void OnAmountChanged(string changedId, int amount)
    {
        if (!string.Equals(changedId?.Trim(), entryId?.Trim(), StringComparison.OrdinalIgnoreCase) ||
            ResourceInventoryManager.Instance == null)
            return;

        Refresh(
            ResourceInventoryManager.Instance.GetDefinition(entryId),
            amount,
            ResourceInventoryManager.Instance.IsExpired(entryId));
    }

    private void OnLanguageChanged(Language language)
    {
        if (ResourceInventoryManager.Instance != null)
            Refresh(
                ResourceInventoryManager.Instance.GetDefinition(entryId),
                ResourceInventoryManager.Instance.GetAmount(entryId),
                ResourceInventoryManager.Instance.IsExpired(entryId));
    }

    private void OnInventoryChanged()
    {
        if (ResourceInventoryManager.Instance != null)
            Refresh(
                ResourceInventoryManager.Instance.GetDefinition(entryId),
                ResourceInventoryManager.Instance.GetAmount(entryId),
                ResourceInventoryManager.Instance.IsExpired(entryId));
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }
}
