using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DailyQuestRewardSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private CanvasGroup canvasGroup;

    public RectTransform RectTransform => transform as RectTransform;
    public CanvasGroup CanvasGroup => canvasGroup != null ? canvasGroup : GetComponent<CanvasGroup>();

    public void Bind(string itemName, int amount, Sprite icon)
    {
        if (itemNameText != null)
            itemNameText.text = itemName;

        if (amountText != null)
            amountText.text = amount > 1 ? $"×{amount}" : string.Empty;

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.gameObject.SetActive(icon != null);
        }
    }

    private void OnDestroy()
    {
        if (RectTransform != null)
            RectTransform.DOKill();
        if (canvasGroup != null)
            canvasGroup.DOKill();
    }
}
