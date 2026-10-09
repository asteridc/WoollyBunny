using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SectionTabButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private CanvasGroup selectionGlow;

    [Header("Colors")]
    [SerializeField] private Color selectedTextColor = Color.white;
    [SerializeField] private Color unselectedTextColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.16f;
    [SerializeField, Range(1f, 1.15f)] private float selectedScale = 1.04f;
    [SerializeField] private Ease animationEase = Ease.OutCubic;

    private SectionSwitcher switcher;
    private int sectionIndex;
    private bool initialized;
    private bool isSelected;

    private void Reset()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<TMP_Text>(true);
    }

    public void Initialize(SectionSwitcher owner, int index)
    {
        if (initialized)
            return;

        switcher = owner;
        sectionIndex = index;

        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError($"SectionTabButton: на объекте {name} не найден Button.", this);
            return;
        }

        if (selectionGlow != null)
        {
            selectionGlow.interactable = false;
            selectionGlow.blocksRaycasts = false;
        }

        button.onClick.AddListener(HandleClick);
        initialized = true;
    }

    public void SetSelected(bool selected, bool instant = false)
    {
        isSelected = selected;
        label?.DOKill();
        label?.rectTransform.DOKill();
        selectionGlow?.DOKill();

        Color targetColor = selected ? selectedTextColor : unselectedTextColor;
        float targetScale = selected ? selectedScale : 1f;
        float targetGlowAlpha = selected ? 1f : 0f;

        if (instant)
        {
            if (label != null)
            {
                label.color = targetColor;
                label.rectTransform.localScale = Vector3.one * targetScale;
            }

            if (selectionGlow != null)
                selectionGlow.alpha = targetGlowAlpha;

            return;
        }

        if (label != null)
        {
            label.DOColor(targetColor, animationDuration).SetEase(animationEase).SetUpdate(true);
            label.rectTransform.DOScale(targetScale, animationDuration).SetEase(animationEase).SetUpdate(true);
        }

        if (selectionGlow != null)
            selectionGlow.DOFade(targetGlowAlpha, animationDuration).SetEase(animationEase).SetUpdate(true);
    }

    private void HandleClick()
    {
        if (initialized && !isSelected)
            switcher.SelectSection(sectionIndex);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}
