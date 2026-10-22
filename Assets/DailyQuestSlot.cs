using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestSlot : MonoBehaviour
{
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button rewardButton;
    [SerializeField] private Graphic completedGraphic;
    [SerializeField] private Image rewardItemImage;
    [SerializeField] private Image rewardCheckmarkImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.gray;
    [SerializeField] private Color claimedRewardColor = new Color(0.35f, 0.75f, 0.45f, 0.65f);
    private DailyQuestManager manager;
    private int questIndex;

    public void Bind(DailyQuestManager questManager, int index)
    {
        manager = questManager;
        questIndex = index;
    }

    public void Refresh(
        DailyQuestDefinition definition,
        int progress,
        bool completed,
        bool rewardClaimed,
        Language language)
    {
        if (definition == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        progress = Mathf.Clamp(progress, 0, definition.requiredCount);

        if (descriptionText != null)
        {
            descriptionText.text = language == Language.English
                ? definition.englishDescription
                : definition.russianDescription;
            descriptionText.color = completed ? completedColor : normalColor;
        }

        if (progressText != null)
        {
            progressText.text = $"{progress} / {definition.requiredCount}";
            progressText.color = completed ? completedColor : normalColor;
        }

        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = definition.requiredCount;
            progressSlider.value = progress;
        }

        if (rewardButton != null)
            rewardButton.interactable = completed && !rewardClaimed;

        if (rewardItemImage != null)
        {
            Sprite rewardIcon = null;
            if (definition.rewardType == DailyQuestRewardType.InventoryEntry &&
                ResourceInventoryManager.Instance != null)
            {
                InventoryEntryDefinition entry = ResourceInventoryManager.Instance.GetDefinition(
                    definition.inventoryEntryId);
                if (entry != null && entry.icon != null)
                    rewardIcon = entry.icon;
            }

            rewardItemImage.sprite = rewardIcon;
            rewardItemImage.gameObject.SetActive(rewardIcon != null);
            rewardItemImage.color = rewardClaimed ? claimedRewardColor : Color.white;
        }

        if (rewardCheckmarkImage != null)
            rewardCheckmarkImage.gameObject.SetActive(rewardClaimed);

        if (completedGraphic != null)
        {
            completedGraphic.DOKill();
            completedGraphic.color = completed ? completedColor : normalColor;
        }

        if (completed)
            ApplyStrikeThrough();
        else if (descriptionText != null)
            descriptionText.text = StripStrikeTags(descriptionText.text);
    }

    public void OnClaimRewardClicked()
    {
        manager?.ClaimReward(questIndex);
    }

    private void ApplyStrikeThrough()
    {
        if (descriptionText == null)
            return;

        descriptionText.text = StripStrikeTags(descriptionText.text);
        descriptionText.text = $"<s>{descriptionText.text}</s>";
    }

    private string StripStrikeTags(string value)
    {
        return value.Replace("<s>", string.Empty).Replace("</s>", string.Empty);
    }
}
