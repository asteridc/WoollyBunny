using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackpackTabButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private CanvasGroup selectionGlow;

    [Header("Colors")]
    [SerializeField] private Color selectedTextColor = Color.white;
    [SerializeField]
    private Color unselectedTextColor =
        new Color(0.45f, 0.45f, 0.45f, 1f);

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.16f;

    [Range(1f, 1.15f)]
    [SerializeField] private float selectedScale = 1.04f;

    [SerializeField] private Ease textEase = Ease.OutCubic;
    [SerializeField] private Ease glowEase = Ease.OutCubic;

    private BackpackSectionController controller;
    private int sectionIndex;

    private bool initialized;
    private bool isSelected;

    private void Reset()
    {
        button = GetComponent<Button>();
        label = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void Initialize(BackpackSectionController sectionController, int index)
    {
        if (initialized)
            return;

        controller = sectionController;
        sectionIndex = index;

        if (button == null)
            button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError(
                $"BackpackTabButton: на объекте {name} не найден Button.",
                this);

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

        Color targetColor =
            selected ? selectedTextColor : unselectedTextColor;

        float targetScale =
            selected ? selectedScale : 1f;

        float targetGlowAlpha =
            selected ? 1f : 0f;

        if (instant)
        {
            if (label != null)
            {
                label.color = targetColor;
                label.rectTransform.localScale =
                    Vector3.one * targetScale;
            }

            if (selectionGlow != null)
                selectionGlow.alpha = targetGlowAlpha;

            return;
        }

        if (label != null)
        {
            label
                .DOColor(targetColor, animationDuration)
                .SetEase(textEase)
                .SetUpdate(true);

            label.rectTransform
                .DOScale(targetScale, animationDuration)
                .SetEase(textEase)
                .SetUpdate(true);
        }

        if (selectionGlow != null)
        {
            selectionGlow
                .DOFade(targetGlowAlpha, animationDuration)
                .SetEase(glowEase)
                .SetUpdate(true);
        }
    }

    private void HandleClick()
    {
        if (!initialized || isSelected)
            return;

        controller.SelectSection(sectionIndex);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}