using UnityEngine;
using TMPro;
using DG.Tweening;

public class TooltipManager : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup tooltipGroup;
    public TextMeshProUGUI tooltipText;
    public TextMeshProUGUI tooltipHeading;

    [Header("Animation")]
    public float fadeDuration = 0.25f;
    public Vector2 offset = new Vector2(10f, 0f);

    private void Awake()
    {
        tooltipGroup.alpha = 0f;
        tooltipGroup.interactable = false;
        tooltipGroup.blocksRaycasts = false;
    }

    public void ShowTooltip(string text, string text2, Vector3 worldPosition)
    {
        tooltipText.text = text;
        tooltipHeading.text = text2;
        tooltipGroup.transform.position = worldPosition + (Vector3)offset;

        tooltipGroup.DOKill();
        tooltipGroup.interactable = true;
        tooltipGroup.blocksRaycasts = true;
        tooltipGroup.DOFade(1f, fadeDuration).SetUpdate(true);
    }

    public void HideTooltip()
    {
        tooltipGroup.DOKill();
        tooltipGroup.DOFade(0f, fadeDuration).SetUpdate(true)
            .OnComplete(() =>
            {
                tooltipGroup.interactable = false;
                tooltipGroup.blocksRaycasts = false;
            });
    }
}
