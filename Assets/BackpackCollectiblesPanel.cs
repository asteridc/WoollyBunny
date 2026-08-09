using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class BackpackCollectiblesPanel : MonoBehaviour
{
    public static BackpackCollectiblesPanel Instance;
    [SerializeField] private BackpackSectionController sectionController;

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
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        rect.localScale = Vector3.one * 0.95f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            Hide();
        }

        if (Input.GetKeyDown(KeyCode.I) && isOpen)
        {
            BackpackSectionController.Instance.SelectSection(2);
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
        isOpen = false;

        screenFade.DOKill();
        canvasGroup.DOKill();
        rect.DOKill();


        screenFade.DOFade(1f, fadeTime)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                canvasGroup.DOFade(0, fadeTime)
                    .SetUpdate(true);

                rect.DOScale(0.95f, scaleDuration)
                    .SetEase(Ease.InBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        screenFade.DOFade(0, fadeTime)
                            .SetUpdate(true);
                    });
            });



        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}