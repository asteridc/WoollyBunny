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
    [SerializeField] private GameObject collectibleImageContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private RectTransform contentRect;

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

    public void Show(
    CollectibleItemBase collectible,
    GameObject sourceVisual)
    {
        if (collectible == null)
        {
            Debug.LogWarning(
                "CollectibleOverview.Show: collectible == null.",
                this
            );

            return;
        }

        IsOpen = true;

        gameObject.SetActive(true);

        canvasGroup.DOKill();
        rect.DOKill();

        // =========================================================
        // COLLECTIBLE VISUAL
        // =========================================================

        if (collectibleImageContainer != null)
        {
            // Удаляем предыдущий визуальный объект.
            for (
                int i = collectibleImageContainer.transform.childCount - 1;
                i >= 0;
                i--
            )
            {
                Destroy(
                    collectibleImageContainer.transform
                        .GetChild(i)
                        .gameObject
                );
            }

            if (sourceVisual != null)
            {
                GameObject clone = Instantiate(
                    sourceVisual,
                    collectibleImageContainer.transform
                );

                clone.SetActive(true);

                RectTransform cloneRect =
                    clone.GetComponent<RectTransform>();

                if (cloneRect != null)
                {
                    cloneRect.localPosition = Vector3.zero;
                    cloneRect.localRotation = Quaternion.identity;
                    cloneRect.localScale = Vector3.one;
                }
            }
            else
            {
                Debug.LogWarning(
                    $"Для collectible '{collectible.name}' " +
                    "не передан sourceVisual.",
                    this
                );
            }
        }

        // =========================================================
        // TEXT / IMAGE
        // =========================================================

        Sprite icon = collectible.GetIcon();

        if (collectibleImage != null)
        {
            collectibleImage.sprite = icon;
            collectibleImage.enabled = icon != null;
            collectibleImage.preserveAspect = true;
        }

        if (titleText != null)
        {
            titleText.text = collectible.GetTitle();
        }

        if (descriptionText != null)
        {
            descriptionText.text = collectible.GetContent();
        }

        // =========================================================
        // LAYOUT
        // =========================================================

        if (descriptionText != null)
        {
            descriptionText.ForceMeshUpdate();

            Canvas.ForceUpdateCanvases();

            LayoutRebuilder.ForceRebuildLayoutImmediate(
                descriptionText.rectTransform
            );
        }

        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(
                contentRect
            );
        }

        // =========================================================
        // ANIMATION
        // =========================================================

        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one * startScale;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

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