using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class ItemAddedUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemTypeText;
    public Image itemIcon;
    public GameObject panel;

    [SerializeField] private CanvasGroup canvasGroup;

    public List<ItemNotification> notifications;
    private int currentIndex = 0;

    public float displayTime = 3f;

    private Tween currentTween;

    void Start()
    {
        panel.SetActive(false);
        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.one * 0.9f;
    }

    public void ShowNotification(int index)
    {
        if (index < 0 || index >= notifications.Count)
        {
            Debug.LogWarning("Index за пределами списка");
            return;
        }

        ShowInternal(notifications[index]);
    }

    public void ShowNotificationDirect(ItemNotification item)
    {
        ShowInternal(item);
    }

    // ---------------- CORE ----------------

    private void ShowInternal(ItemNotification item)
    {
        // Заполняем данные
        itemNameText.text = item.itemName;
        itemTypeText.text = item.itemType;
        itemTypeText.color = GetColorByRarity(item.rarity);
        itemIcon.sprite = item.itemIcon;

        panel.SetActive(true);

        // Убиваем прошлую анимацию
        currentTween?.Kill();

        canvasGroup.alpha = 0f;
        panel.transform.localScale = Vector3.one * 0.9f;

        // Анимация
        currentTween = DOTween.Sequence()
            .Append(canvasGroup.DOFade(1f, 0.35f).SetEase(Ease.OutQuad))
            .Join(panel.transform.DOScale(1f, 0.35f).SetEase(Ease.OutQuad))
            .AppendInterval(displayTime)
            .Append(canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InQuad))
            .Join(panel.transform.DOScale(0.9f, 0.25f).SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                panel.SetActive(false);
            });
    }

    private Color GetColorByRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.gray;
            case ItemRarity.Rare: return new Color(0.2f, 0.6f, 1f);
            case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case ItemRarity.Legendary: return new Color(1f, 0.6f, 0f);
            default: return Color.white;
        }
    }
}


public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}