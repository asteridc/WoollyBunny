using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResourceItemView : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text rarityText;

    private InventoryEntryDefinition definition;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    public void Bind(InventoryEntryDefinition itemDefinition, int amount, bool expired)
    {
        definition = itemDefinition;
        if (definition == null)
            return;
        canvasGroup = GetComponent<CanvasGroup>();

        if (iconImage != null)
            iconImage.sprite = definition.icon;
        if (nameText != null)
            nameText.text = LanguageManager.CurrentLanguage == Language.English
                ? definition.englishName
                : definition.russianName;
        if (amountText != null)
            amountText.text = definition.type == InventoryEntryType.UniqueItem ? string.Empty : amount.ToString();
        if (rarityText != null)
            rarityText.text = GetLocalizedRarityText(
                definition.rarity,
                LanguageManager.CurrentLanguage == Language.English);

        if (canvasGroup != null)
            canvasGroup.alpha = expired ? 0.5f : 1f;
    }

    private void Update()
    {
        if (definition == null || canvasGroup == null || ResourceInventoryManager.Instance == null)
            return;

        canvasGroup.alpha = ResourceInventoryManager.Instance.IsExpired(definition.id) ? 0.5f : 1f;
    }

    private void OnLanguageChanged(Language language)
    {
        if (definition != null)
            Bind(definition, 1, ResourceInventoryManager.Instance != null &&
                ResourceInventoryManager.Instance.IsExpired(definition.id));
    }

    public static string GetLocalizedRarityText(ResourceRarity rarity, bool english)
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
