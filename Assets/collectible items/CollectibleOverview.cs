using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class CollectibleOverview : MonoBehaviour
{
    public static CollectibleOverview Instance;

    [Header("Content")]
    [SerializeField] private Image collectibleImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private float startScale = 0.95f;

    private CanvasGroup canvasGroup;
    private RectTransform rect;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        Instance = this;
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        rect.localScale = Vector3.one * startScale;
    }

    public void Show(Sprite image, string title, string description)
    {
        IsOpen = true;

        gameObject.SetActive(true);

        canvasGroup.DOKill();
        rect.DOKill();

        // Контент
        if (collectibleImage != null)
        {
            collectibleImage.sprite = image;
            collectibleImage.enabled = image != null;
            collectibleImage.preserveAspect = true;
        }

        if (titleText != null)
            titleText.text = title;

        if (descriptionText != null)
            descriptionText.text = description;

        // Начальное состояние
        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one * startScale;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        // Анимация появления
        canvasGroup
            .DOFade(1f, fadeDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);

        rect
            .DOScale(1f, scaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void Hide()
    {
        if (!IsOpen)
            return;

        IsOpen = false;

        canvasGroup.DOKill();
        rect.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence sequence = DOTween.Sequence();

        sequence
            .Join(
                canvasGroup
                    .DOFade(0f, fadeDuration)
                    .SetEase(Ease.InQuad)
            )
            .Join(
                rect
                    .DOScale(startScale, scaleDuration)
                    .SetEase(Ease.InBack)
            )
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(false);
            });
    }
}