using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BackpackCollectiblesPanel : MonoBehaviour
{
    public static BackpackCollectiblesPanel Instance;

    [SerializeField] private BackpackSectionController sectionController;

    [Header("Collectibles Section")]
    [SerializeField] private GameObject collectiblesSection;
    [SerializeField] private CanvasGroup collectiblesSectionCanvasGroup;
    [SerializeField] private RectTransform collectiblesSectionRect;

    [Header("Collectible Overview")]
    [SerializeField] private CollectibleOverview collectibleOverview;

    [Header("Transition")]
    [SerializeField] private CanvasGroup screenFade;
    [SerializeField] private float fadeTime = 0.25f;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.25f;
    [SerializeField] private float switchDuration = 0.3f;

    public CanvasGroup canvasGroup;
    private RectTransform rect;

    public bool isOpen = false;

    public bool IsOpen => isOpen;

    public void SetSectionOpenState(bool value)
    {
        isOpen = value;
    }

    private void Awake()
    {
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        rect.localScale = Vector3.one * 0.95f;

        if (collectiblesSectionCanvasGroup != null)
        {
            collectiblesSectionCanvasGroup.alpha = 1f;
            collectiblesSectionCanvasGroup.interactable = true;
            collectiblesSectionCanvasGroup.blocksRaycasts = true;
        }
    }

    public void Toggle()
    {
        if (isOpen)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        isOpen = true;

        if (BackpackSectionController.Instance != null)
            BackpackSectionController.Instance.IsBackpackOpen();

        screenFade.DOKill();
        canvasGroup.DOKill();
        rect.DOKill();

        screenFade.alpha = 0;

        screenFade.DOFade(1f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                gameObject.SetActive(true);

                if (sectionController != null)
                    sectionController.OpenCollectiblesSection();

                canvasGroup.alpha = 0;
                rect.localScale = Vector3.one * 0.95f;

                canvasGroup.DOFade(1, fadeTime)
                    .SetUpdate(true);

                rect.DOScale(1, scaleDuration)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true);

                screenFade.DOFade(0, fadeTime)
                    .SetDelay(0.1f)
                    .SetUpdate(true);
            });

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        Debug.Log("BACKPACK COLLECTIBLES HIDE CALLED");

        isOpen = false;

        if (collectibleOverview != null &&
            collectibleOverview.IsOpen)
        {
            collectibleOverview.Hide();
        }

        collectiblesSectionCanvasGroup?.DOKill();

        screenFade.DOKill();
        canvasGroup.DOKill();
        rect.DOKill();

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        screenFade
            .DOFade(1f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup
                    .DOFade(0f, fadeTime)
                    .SetUpdate(true);

                rect
                    .DOScale(0.95f, scaleDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        screenFade
                            .DOFade(0f, fadeTime)
                            .SetUpdate(true);
                    });
            });
    }


    // ============================================================
    // COLLECTIBLE OVERVIEW
    // ============================================================

    public void OpenCollectibleOverview(
    CollectibleItemBase collectible,
    GameObject collectibleVisual)
    {
        Debug.Log("OPEN COLLECTIBLE OVERVIEW");

        if (!isOpen)
        {
            Debug.LogWarning(
                "Collectibles panel is not open."
            );

            return;
        }

        if (collectibleOverview == null)
        {
            Debug.LogWarning(
                "CollectibleOverview is not assigned.",
                this
            );

            return;
        }

        if (collectible == null)
        {
            Debug.LogWarning(
                "Collectible is null.",
                this
            );

            return;
        }

        HideCollectiblesSection();

        collectibleOverview.Show(
            collectible,
            collectibleVisual
        );
    }

    public void CloseCollectibleOverview()
    {
        if (!IsCollectibleOverviewOpen)
            return;

        collectibleOverview.Hide();

        ShowCollectiblesSection();
    }


    // ============================================================
    // COLLECTIBLES SECTION
    // ============================================================

    private void HideCollectiblesSection()
    {
        if (collectiblesSectionCanvasGroup == null)
            return;

        collectiblesSectionCanvasGroup.DOKill();

        collectiblesSectionCanvasGroup.interactable = false;
        collectiblesSectionCanvasGroup.blocksRaycasts = false;

        collectiblesSectionCanvasGroup
            .DOFade(0f, switchDuration)
            .SetEase(Ease.InQuad)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                if (collectiblesSection != null)
                    collectiblesSection.SetActive(false);
            });
    }

    private void ShowCollectiblesSection()
    {
        if (collectiblesSectionCanvasGroup == null)
            return;

        if (collectiblesSection != null)
            collectiblesSection.SetActive(true);

        collectiblesSectionCanvasGroup.DOKill();

        collectiblesSectionCanvasGroup.alpha = 0f;
        collectiblesSectionCanvasGroup.interactable = true;
        collectiblesSectionCanvasGroup.blocksRaycasts = true;

        collectiblesSectionCanvasGroup
            .DOFade(1f, switchDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }
  
    public bool IsCollectibleOverviewOpen =>
    collectibleOverview != null &&
    collectibleOverview.IsOpen;
}