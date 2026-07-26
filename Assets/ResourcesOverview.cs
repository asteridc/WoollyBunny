using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ResourcesOverview : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float showDuration = 0.35f;
    [SerializeField] private float hiddenOffset = 500f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;

    private Vector2 shownPosition;
    private Vector2 hiddenPosition;

    private bool isVisible;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        shownPosition = rect.anchoredPosition;
        hiddenPosition = shownPosition + Vector2.right * hiddenOffset;

        rect.anchoredPosition = hiddenPosition;

        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void Show()
    {
        if (isVisible)
            return;

        isVisible = true;

        rect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        rect.DOAnchorPos(shownPosition, showDuration)
            .SetEase(Ease.OutExpo).SetUpdate(true);

        canvasGroup.DOFade(1, showDuration).SetUpdate(true);
    }

    public void Hide()
    {
        isVisible = false;

        rect.DOKill();
        canvasGroup.DOKill();

        rect.DOAnchorPos(hiddenPosition, showDuration)
            .SetEase(Ease.InExpo).SetUpdate(true);

        canvasGroup.DOFade(0, showDuration)
            .OnComplete(() =>
            {
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }).SetUpdate(true);
    }
}
