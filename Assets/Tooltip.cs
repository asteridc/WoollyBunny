using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Location")]
    [SerializeField] private Vector2 cursorOffset = new(24, 24);
    [SerializeField] private float screenPadding = 12f;

    [Header("Animation")]
    [SerializeField] private float fadeDuration = 0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource addictScreamSource;
    [SerializeField] private AudioSource childScreamSource;
    [SerializeField] private float finalEventDelay = 0.5f;
    [SerializeField] private float childScreamDelay = 0.35f;

    private RectTransform rect;
    [SerializeField] private Canvas canvas;
    private CanvasGroup canvasGroup;

    private bool isVisible;
    private bool pointerInside;
    private Coroutine pendingHideCoroutine;
    [SerializeField] private float pointerTransferDelay = 0.12f;

    public bool IsPointerInside => pointerInside;



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
        if (pendingHideCoroutine != null)
        {
            StopCoroutine(pendingHideCoroutine);
            pendingHideCoroutine = null;
        }

        gameObject.SetActive(true);

        canvasGroup.DOKill();
        rect.DOKill();

        canvasGroup.alpha = 0f;
        rect.localScale = Vector3.one * 0.9f;

        canvasGroup.DOFade(1, fadeDuration).SetUpdate(true);
        rect.DOScale(1, fadeDuration).SetEase(Ease.OutBack).SetUpdate(true);
        isVisible = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerInside = true;
        if (pendingHideCoroutine != null)
        {
            StopCoroutine(pendingHideCoroutine);
            pendingHideCoroutine = null;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        Hide();
    }


    public void Hide()
    {
        if (!gameObject.activeInHierarchy)
        {
            isVisible = false;
            return;
        }

        if (pendingHideCoroutine != null)
            StopCoroutine(pendingHideCoroutine);

        pendingHideCoroutine = StartCoroutine(HideAfterPointerTransfer());
    }

    private IEnumerator HideAfterPointerTransfer()
    {
        yield return new WaitForSecondsRealtime(pointerTransferDelay);
        pendingHideCoroutine = null;

        if (pointerInside)
            yield break;

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
