using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldToFillAction : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler
{
    [Header("Progress UI")]
    [SerializeField] private RectTransform barRect;
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform filledContentClip;
    [SerializeField] private Graphic[] contrastTargets;
    [SerializeField] private RectTransform inputArea;
    [SerializeField] private Color unfilledColor = Color.white;
    [SerializeField] private Color filledColor = Color.black;

    [Header("Hold")]
    [SerializeField, Min(0.1f)] private float holdDuration = 1f;
    [SerializeField] private bool useUnscaledTime = true;
    [SerializeField] private bool resetProgressOnRelease = true;
    [SerializeField] private bool repeatAfterComplete;
    [SerializeField] private bool cancelOnPointerExit;

    [Header("Action")]
    [SerializeField] private UnityEvent onCompleted;

    private bool isHolding;
    private bool isComplete;
    private float heldTime;
    private Graphic[] cachedTargets;
    private Color[] originalColors;

    public float Progress => holdDuration <= 0f
        ? 1f
        : Mathf.Clamp01(heldTime / holdDuration);

    public bool IsComplete => isComplete;

    private void Awake()
    {
        if (fillImage != null && fillImage.type != Image.Type.Filled)
        {
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        }

        cachedTargets = contrastTargets;
        if (cachedTargets != null)
        {
            originalColors = new Color[cachedTargets.Length];
            for (int i = 0; i < cachedTargets.Length; i++)
            {
                if (cachedTargets[i] != null)
                    originalColors[i] = cachedTargets[i].color;
            }
        }

        UpdateProgressVisuals();
    }

    private void Update()
    {
        UpdateMouseHoldState();

        if (!isHolding || isComplete)
            return;

        heldTime += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (heldTime >= holdDuration)
        {
            heldTime = holdDuration;
            isComplete = true;
            isHolding = repeatAfterComplete;
            onCompleted?.Invoke();
        }

        UpdateProgressVisuals();
    }

    private void UpdateMouseHoldState()
    {
        if (isComplete)
            return;

        bool pointerInside = inputArea == null || IsPointerInsideInputArea();

        if (Input.GetMouseButton(0) && pointerInside)
        {
            isHolding = true;
        }
        else if (isHolding && (!Input.GetMouseButton(0) ||
                               (cancelOnPointerExit && !pointerInside)))
        {
            StopHolding();
        }
    }

    private bool IsPointerInsideInputArea()
    {
        Canvas canvas = inputArea.GetComponentInParent<Canvas>();
        Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        return RectTransformUtility.RectangleContainsScreenPoint(
            inputArea,
            Input.mousePosition,
            eventCamera);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left || isComplete)
            return;

        isHolding = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cancelOnPointerExit)
            StopHolding();
    }

    public void ResetAction()
    {
        isHolding = false;
        isComplete = false;
        heldTime = 0f;
        UpdateProgressVisuals();
    }

    private void StopHolding()
    {
        isHolding = false;
        if (resetProgressOnRelease && !isComplete)
        {
            heldTime = 0f;
            UpdateProgressVisuals();
        }
    }

    private void UpdateProgressVisuals()
    {
        float progress = holdDuration <= 0f
            ? 1f
            : Mathf.Clamp01(heldTime / holdDuration);

        if (fillImage != null)
            fillImage.fillAmount = progress;

        if (barRect != null && filledContentClip != null)
        {
            float width = barRect.rect.width * progress;
            filledContentClip.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                width);
        }

        if (contrastTargets != null && barRect != null)
        {
            Vector3[] barCorners = new Vector3[4];
            barRect.GetWorldCorners(barCorners);
            float splitX = Mathf.Lerp(barCorners[0].x, barCorners[3].x, progress);

            foreach (Graphic target in contrastTargets)
            {
                if (target == null)
                    continue;

                Vector3 center = target.rectTransform.TransformPoint(
                    target.rectTransform.rect.center);
                bool insideFilled = center.x <= splitX;
                Color targetColor = insideFilled ? filledColor : unfilledColor;
                if (originalColors != null && originalColors.Length > 0)
                {
                    int index = System.Array.IndexOf(cachedTargets, target);
                    Color original = index >= 0 ? originalColors[index] : target.color;
                    targetColor.a = original.a;
                }

                target.color = targetColor;
            }
        }
    }
}
