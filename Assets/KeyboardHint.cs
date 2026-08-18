using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class KeyboardHint : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Input")]
    [SerializeField] private KeyCode key = KeyCode.Escape;

    [Header("Keyboard Action")]
    [Tooltip("Действие, которое выполняется при реальном нажатии клавиши.")]
    [SerializeField] private UnityEvent keyboardAction;

    [Header("Mouse Action")]
    [Tooltip("Действие, которое выполняется при клике мышкой по подсказке.")]
    [SerializeField] private UnityEvent clickAction;

    [Header("Visual")]
    [Tooltip("Объект клавиши, который будет визуально прожиматься.")]
    [SerializeField] private RectTransform keyVisual;

    [Tooltip("Графический компонент клавиши для подсветки.")]
    [SerializeField] private Graphic highlightTarget;

    [SerializeField] private Color normalColor = Color.white;

    [SerializeField]
    private Color highlightedColor =
        new Color(1f, 0.85f, 0.35f, 1f);

    [Header("Press Animation")]
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float pressDownDuration = 0.08f;
    [SerializeField] private float pressUpDuration = 0.12f;

    [Header("Hover")]
    [SerializeField] private bool useHover = true;

    [SerializeField]
    private Color hoverColor =
        new Color(1f, 1f, 1f, 0.85f);

    [SerializeField] private float hoverDuration = 0.08f;

    private Vector3 originalScale;
    private bool isPointerInside;

    private void Awake()
    {
        if (keyVisual == null)
            keyVisual = transform as RectTransform;

        if (keyVisual != null)
            originalScale = keyVisual.localScale;

        if (highlightTarget != null)
            highlightTarget.color = normalColor;
    }

    private void Update()
    {
        if (!isActiveAndEnabled)
            return;

        if (Input.GetKeyDown(key))
        {
            HandleKeyboardPress();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        HandleMouseClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerInside = true;

        if (!useHover || highlightTarget == null)
            return;

        highlightTarget.DOKill();

        highlightTarget
            .DOColor(hoverColor, hoverDuration)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerInside = false;

        if (!useHover || highlightTarget == null)
            return;

        highlightTarget.DOKill();

        highlightTarget
            .DOColor(normalColor, hoverDuration)
            .SetUpdate(true);
    }

    private void HandleKeyboardPress()
    {
        // ВАЖНО:
        // Никакой анимации, hover или изменения визуала.
        keyboardAction?.Invoke();
    }

    private void HandleMouseClick()
    {
        // Только мышь вызывает визуальную реакцию.
        PlayPressAnimation();

        clickAction?.Invoke();
    }

    private void PlayPressAnimation()
    {
        AnimateKey();
        AnimateHighlight();
    }

    private void AnimateKey()
    {
        if (keyVisual == null)
            return;

        keyVisual.DOKill();

        keyVisual
            .DOScale(
                originalScale * pressedScale,
                pressDownDuration
            )
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                keyVisual
                    .DOScale(originalScale, pressUpDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);
            });
    }

    private void AnimateHighlight()
    {
        if (highlightTarget == null)
            return;

        highlightTarget.DOKill();

        highlightTarget
            .DOColor(highlightedColor, hoverDuration)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                Color targetColor =
                    isPointerInside && useHover
                        ? hoverColor
                        : normalColor;

                highlightTarget
                    .DOColor(targetColor, hoverDuration)
                    .SetUpdate(true);
            });
    }

    public void TriggerClickAction()
    {
        HandleMouseClick();
    }
}