using DG.Tweening;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Tooltip : MonoBehaviour
{
    [Header("Location")]
    [SerializeField] private Vector2 cursorOffset = new(24, 24);
    [SerializeField] private float screenPadding = 12f;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.15f;

    private RectTransform rect;
    [SerializeField] private Canvas canvas;
    private CanvasGroup canvasGroup;

    private bool isVisible;



    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }


    private void Update()
    {
        if (!isVisible) return;
        FollowCursor();
    }


    public void Show()
    {
        gameObject.SetActive(true);

        canvasGroup.DOKill();
        rect.DOKill();

        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one * 0.9f;

        canvasGroup.DOFade(1, fadeDuration).SetUpdate(true);
        rect.DOScale(1, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true);
        isVisible = true;
    }


    public void Hide()
    {
        isVisible = false;
        canvasGroup.DOKill();

        canvasGroup.DOFade(0, fadeDuration).OnComplete(() => gameObject.SetActive(false)).SetUpdate(true);

    }


    private void FollowCursor()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        Vector2 mouse = Input.mousePosition;
        Vector2 size = rect.rect.size;

        float x = mouse.x + 24;
        float y = mouse.y - 24;

        // Правая граница
        if (x + size.x > Screen.width - screenPadding)
            x = mouse.x - size.x - 24;

        // Нижняя граница
        if (y - size.y < screenPadding)
            y = mouse.y + size.y + 24;

        rect.position = new Vector3(x, y, 0);
    }
}
