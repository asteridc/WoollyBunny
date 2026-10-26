using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles only the animated visibility of the pumpkin lantern UI.
/// Keep this component on an active object outside the panel it controls.
/// </summary>
public sealed class PumpkinLanternAnimator : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Panel")]
    [SerializeField] private CanvasGroup panelCanvasGroup;
    [SerializeField] private RectTransform panelTransform;

    [Header("Animation")]
    [SerializeField, Min(0.01f)] private float animationDuration = 0.25f;
    [SerializeField, Range(0.8f, 0.99f)] private float hiddenScale = 0.94f;

    private Sequence currentAnimation;
    private bool isOpen;
    private bool isAnimating;

    private void Awake()
    {
        HideInstant();
    }

    private void OnEnable()
    {
        if (openButton != null)
            openButton.onClick.AddListener(Open);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnDisable()
    {
        if (openButton != null)
            openButton.onClick.RemoveListener(Open);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);

        currentAnimation?.Kill();
        currentAnimation = null;

        // Reset the panel if its parent UI is disabled while an animation is running.
        HideInstant();
    }

    private void OnDestroy()
    {
        currentAnimation?.Kill();
    }

    public void Open()
    {
        if (isOpen || isAnimating || !ReferencesAreValid())
            return;

        isOpen = true;
        isAnimating = true;

        currentAnimation?.Kill();
        panelCanvasGroup.DOKill();
        panelTransform.DOKill();

        panelCanvasGroup.gameObject.SetActive(true);
        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        // Block clicks behind the panel even while it is fading in.
        panelCanvasGroup.blocksRaycasts = true;
        panelTransform.localScale = Vector3.one * hiddenScale;

        currentAnimation = DOTween.Sequence().SetUpdate(true);
        currentAnimation.Join(panelCanvasGroup.DOFade(1f, animationDuration));
        currentAnimation.Join(
            panelTransform.DOScale(1f, animationDuration)
                .SetEase(Ease.OutCubic));
        currentAnimation.OnComplete(() =>
        {
            isAnimating = false;
            if (panelCanvasGroup != null)
                panelCanvasGroup.interactable = true;
        });
    }

    public void Close()
    {
        if (!isOpen || isAnimating || !ReferencesAreValid())
            return;

        isOpen = false;
        isAnimating = true;
        panelCanvasGroup.interactable = false;
        // Keep blocking raycasts until the closing animation finishes.
        panelCanvasGroup.blocksRaycasts = true;

        currentAnimation?.Kill();
        panelCanvasGroup.DOKill();
        panelTransform.DOKill();

        currentAnimation = DOTween.Sequence().SetUpdate(true);
        currentAnimation.Join(panelCanvasGroup.DOFade(0f, animationDuration));
        currentAnimation.Join(
            panelTransform.DOScale(hiddenScale, animationDuration)
                .SetEase(Ease.InCubic));
        currentAnimation.OnComplete(() =>
        {
            HideInstant();
            isAnimating = false;
        });
    }

    private bool ReferencesAreValid()
    {
        if (panelCanvasGroup == null || panelTransform == null)
        {
            Debug.LogWarning(
                "PumpkinLanternPanelAnimator: assign Panel Canvas Group and Panel Transform in the Inspector.",
                this);
            return false;
        }

        if (panelCanvasGroup.gameObject == gameObject)
        {
            Debug.LogError(
                "PumpkinLanternPanelAnimator must be outside the panel it controls; otherwise disabling the panel also disables this controller.",
                this);
            return false;
        }

        return true;
    }

    private void HideInstant()
    {
        if (panelCanvasGroup == null || panelCanvasGroup.gameObject == gameObject)
            return;

        panelCanvasGroup.DOKill();
        if (panelTransform != null)
        {
            panelTransform.DOKill();
            panelTransform.localScale = Vector3.one * hiddenScale;
        }

        panelCanvasGroup.alpha = 0f;
        panelCanvasGroup.interactable = false;
        panelCanvasGroup.blocksRaycasts = false;
        panelCanvasGroup.gameObject.SetActive(false);

        isOpen = false;
        isAnimating = false;
    }
}