using UnityEngine;
using TMPro;
using DG.Tweening;

public class TooltipManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup tooltipGroup;
    [SerializeField] private TextMeshProUGUI tooltipHeading;
    [SerializeField] private TextMeshProUGUI tooltipText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.25f;
    [SerializeField] private Vector2 offset = new Vector2(10f, 0f);

    private TooltipData currentTooltip;

    private void Awake()
    {
        tooltipGroup.alpha = 0f;
        tooltipGroup.interactable = false;
        tooltipGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += RefreshText;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= RefreshText;
    }

    // ===== PUBLIC API =====

    public void ShowTooltip(TooltipData data, Vector3 worldPosition)
    {
        if (data == null) return;

        currentTooltip = data;
        ApplyText();

        tooltipGroup.transform.position = worldPosition + (Vector3)offset;

        tooltipGroup.DOKill();
        tooltipGroup.interactable = true;
        tooltipGroup.blocksRaycasts = true;
        tooltipGroup.DOFade(1f, fadeDuration).SetUpdate(true);
    }

    public void HideTooltip()
    {
        currentTooltip = null;

        tooltipGroup.DOKill();
        tooltipGroup.DOFade(0f, fadeDuration).SetUpdate(true)
            .OnComplete(() =>
            {
                tooltipGroup.interactable = false;
                tooltipGroup.blocksRaycasts = false;
            });
    }

    // ===== LANGUAGE REACTION =====

    private void RefreshText(Language _)
    {
        if (currentTooltip == null) return;
        ApplyText();
    }

    private void ApplyText()
    {
        tooltipHeading.text = currentTooltip.GetHeading();
        tooltipText.text = currentTooltip.GetText();
    }
}
