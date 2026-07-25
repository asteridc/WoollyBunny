using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BackpackItemsPanel : MonoBehaviour
{
    public static BackpackItemsPanel Instance;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.15f;
    [SerializeField] private float scaleDuration = 0.25f;


    [Header("References")]
    [SerializeField] private WeaponOverview weaponOverview;


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
        if (Input.GetKeyDown(KeyCode.I))
        {
            Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            Hide();
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

        gameObject.SetActive(true);

        canvasGroup.DOKill();
        rect.DOKill();


        canvasGroup.alpha = 0;
        rect.localScale = Vector3.one * 0.95f;


        canvasGroup.DOFade(1, fadeDuration).SetUpdate(true);

        rect.DOScale(1, scaleDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        Time.timeScale = 0f;
    }


    public void Hide()
    {
        isOpen = false;


        if (weaponOverview != null)
            weaponOverview.Hide();


        canvasGroup.DOKill();
        rect.DOKill();


        canvasGroup.DOFade(0, fadeDuration).SetUpdate(true);


        //rect.DOScale(0.95f, scaleDuration)
        //    .SetEase(Ease.InBack)
        //    .OnComplete(() =>
        //    {
        //        canvasGroup.DOFade(0, fadeDuration);

        //        rect.DOScale(0.95f, scaleDuration)
        //            .SetEase(Ease.InBack)
        //            .SetUpdate(true);
        //    });

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        Time.timeScale = 1f;
    }
}