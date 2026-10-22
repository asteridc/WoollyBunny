using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuestRewardNotificationUI : MonoBehaviour
{
    [Header("Frame")]
    [SerializeField] private CanvasGroup frameCanvasGroup;
    [SerializeField, Min(0.01f)] private float frameAnimationDuration = 0.3f;
    [SerializeField, Min(0f)] private float displayDuration = 4f;

    [Header("Reward Slots")]
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private DailyQuestRewardSlotUI slotPrefab;
    [SerializeField, Min(1)] private int maxVisibleSlots = 4;
    [SerializeField, Min(1f)] private float slotSpacing = 150f;
    [SerializeField, Min(0.01f)] private float slotAnimationDuration = 0.3f;

    private readonly List<VisibleReward> visibleRewards = new List<VisibleReward>();
    private Tween frameEnterTween;
    private Tween frameHideTween;

    private class VisibleReward
    {
        public DailyQuestDefinition Definition;
        public Sprite Icon;
        public DailyQuestRewardSlotUI Slot;
    }

    private void Awake()
    {
        if (frameCanvasGroup != null)
            frameCanvasGroup.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        frameEnterTween?.Kill();
        frameHideTween?.Kill();
    }

    public void ShowReward(DailyQuestDefinition definition)
    {
        if (definition == null || frameCanvasGroup == null || slotContainer == null || slotPrefab == null)
        {
            Debug.LogWarning("DailyQuestRewardNotificationUI: настройте фрейм, контейнер и префаб слота в Inspector.", this);
            return;
        }

        Sprite icon = GetRewardIcon(definition);
        bool wasHidden = !frameCanvasGroup.gameObject.activeSelf;
        frameHideTween?.Kill();
        frameCanvasGroup.gameObject.SetActive(true);

        if (wasHidden)
        {
            frameCanvasGroup.alpha = 0f;
            frameCanvasGroup.transform.localScale = Vector3.one * 0.9f;
        }

        frameEnterTween?.Kill();
        frameEnterTween = DOTween.Sequence()
            .Append(frameCanvasGroup.DOFade(1f, frameAnimationDuration).SetEase(Ease.OutQuad))
            .Join(frameCanvasGroup.transform.DOScale(1f, frameAnimationDuration).SetEase(Ease.OutBack));

        if (visibleRewards.Count >= Mathf.Max(1, maxVisibleSlots))
            RemoveOldestReward();

        for (int i = 0; i < visibleRewards.Count; i++)
        {
            RectTransform rectTransform = visibleRewards[i].Slot.RectTransform;
            rectTransform.DOKill();
            rectTransform.DOAnchorPosX(rectTransform.anchoredPosition.x - slotSpacing, slotAnimationDuration)
                .SetEase(Ease.OutCubic);
        }

        DailyQuestRewardSlotUI slot = Instantiate(slotPrefab, slotContainer);
        RectTransform newSlotTransform = slot.RectTransform;
        newSlotTransform.anchoredPosition = new Vector2(slotSpacing, 0f);
        newSlotTransform.localScale = Vector3.one * 0.8f;
        if (slot.CanvasGroup != null)
            slot.CanvasGroup.alpha = 0f;

        VisibleReward reward = new VisibleReward
        {
            Definition = definition,
            Icon = icon,
            Slot = slot
        };
        visibleRewards.Add(reward);
        RefreshRewardSlot(reward, LanguageManager.CurrentLanguage);

        Sequence slotEnterTween = DOTween.Sequence()
            .Join(newSlotTransform.DOAnchorPosX(0f, slotAnimationDuration).SetEase(Ease.OutCubic))
            .Join(newSlotTransform.DOScale(1f, slotAnimationDuration).SetEase(Ease.OutBack));
        if (slot.CanvasGroup != null)
            slotEnterTween.Join(slot.CanvasGroup.DOFade(1f, slotAnimationDuration));

        ScheduleHide();
    }

    private void ScheduleHide()
    {
        frameHideTween?.Kill();
        frameHideTween = DOTween.Sequence()
            .AppendInterval(displayDuration)
            .Append(frameCanvasGroup.DOFade(0f, frameAnimationDuration).SetEase(Ease.InQuad))
            .Join(frameCanvasGroup.transform.DOScale(0.9f, frameAnimationDuration).SetEase(Ease.InBack))
            .OnComplete(ClearRewards);
    }

    private void ClearRewards()
    {
        if (frameCanvasGroup != null)
            frameCanvasGroup.gameObject.SetActive(false);

        foreach (VisibleReward reward in visibleRewards)
        {
            if (reward.Slot != null)
                Destroy(reward.Slot.gameObject);
        }

        visibleRewards.Clear();
    }

    private void RemoveOldestReward()
    {
        VisibleReward oldest = visibleRewards[0];
        visibleRewards.RemoveAt(0);
        if (oldest.Slot != null)
            Destroy(oldest.Slot.gameObject);
    }

    private void OnLanguageChanged(Language language)
    {
        foreach (VisibleReward reward in visibleRewards)
            RefreshRewardSlot(reward, language);
    }

    private void RefreshRewardSlot(VisibleReward reward, Language language)
    {
        reward.Slot.Bind(GetLocalizedRewardName(reward.Definition, language),
            reward.Definition.rewardAmount, reward.Icon);
    }

    private static Sprite GetRewardIcon(DailyQuestDefinition definition)
    {
        if (definition.rewardType != DailyQuestRewardType.InventoryEntry ||
            ResourceInventoryManager.Instance == null)
            return null;

        InventoryEntryDefinition entry = ResourceInventoryManager.Instance.GetDefinition(
            definition.inventoryEntryId);
        return entry != null ? entry.icon : null;
    }

    private static string GetLocalizedRewardName(DailyQuestDefinition definition, Language language)
    {
        bool english = language == Language.English;
        switch (definition.rewardType)
        {
            case DailyQuestRewardType.StoryExperience:
                return english ? "Story experience" : "Опыт истории";
            case DailyQuestRewardType.BloodthirstPoints:
                return english ? "Bloodthirst points" : "Очки кровожадности";
            case DailyQuestRewardType.NoblePoints:
                return english ? "Noble points" : "Очки благородства";
            case DailyQuestRewardType.LovePoints:
                return english ? "Love points" : "Очки любви";
            case DailyQuestRewardType.InventoryEntry:
                InventoryEntryDefinition entry = ResourceInventoryManager.Instance != null
                    ? ResourceInventoryManager.Instance.GetDefinition(definition.inventoryEntryId)
                    : null;
                if (entry != null)
                {
                    string itemName = english ? entry.englishName : entry.russianName;
                    if (!string.IsNullOrWhiteSpace(itemName))
                        return itemName;
                }
                return english ? "Item" : "Предмет";
            case DailyQuestRewardType.Custom:
                string localizedName = english
                    ? definition.englishRewardName
                    : definition.russianRewardName;
                if (!string.IsNullOrWhiteSpace(localizedName))
                    return localizedName;
                return english ? "Reward" : "Награда";
            default:
                return english ? "Reward" : "Награда";
        }
    }
}
