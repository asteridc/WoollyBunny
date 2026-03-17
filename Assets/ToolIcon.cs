using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class ToolIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ToolType toolType;
    public Image iconImage;
    public Sprite iconSprite;

    public ChainSlot CurrentSlot;

    public RectTransform rectTransform;
    public RectTransform RectTransform => rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 originalAnchoredPosition;
    public Transform originalParent;
    public ChainSlot originalParentSlot;
    private bool originalSlotInitialized = false;
    private bool dropHandledThisFrame = false;

    public static List<ChainSlot> allSlots = new List<ChainSlot>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        InitializeOriginalSlotIfNeeded();
    }

    void OnEnable()
    {
        InitializeOriginalSlotIfNeeded();
    }

    private void InitializeOriginalSlotIfNeeded()
    {
        if (originalSlotInitialized)
            return;

        if (originalParent == null)
            originalParent = transform.parent;

        if (originalParentSlot == null)
            originalParentSlot = GetComponentInParent<ChainSlot>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (originalParentSlot != null && originalParentSlot.toolInSlot == null)
        {
            originalParentSlot.toolInSlot = this;
            originalParentSlot.currentTool = toolType;
        }

        if (CurrentSlot == null)
            CurrentSlot = originalParentSlot;

        originalSlotInitialized = true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalAnchoredPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        CurrentSlot = originalParent.GetComponent<ChainSlot>();

        canvasGroup.blocksRaycasts = false;

        foreach (var slot in allSlots)
            slot.SetHighlight(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position += (Vector3)eventData.delta;

        // Подсветка подходящих слотов
        foreach (var slot in allSlots)
        {
            float dist = Vector3.Distance(rectTransform.position, slot.transform.position);
            bool isClose = dist < 40f;
            slot.SetHighlight(isClose);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (dropHandledThisFrame)
        {
            dropHandledThisFrame = false;
            ClearHighlights();
            return;
        }

        ChainSlot targetSlot = GetNearestSlot();
        bool success = false;

        if (targetSlot != null)
        {
            success = TryPlaceInSlot(targetSlot, allowSwap: true);
        }

        if (!success)
        {
            // Возврат назад
            ReturnToStartSlot();
        }

        ClearHighlights();
    }

    private bool TryPlaceInSlot(ChainSlot slot, bool allowSwap)
    {
        if (slot == null)
            return false;

        if (slot.toolInSlot != null && slot.toolInSlot != this)
        {
            if (!allowSwap || CurrentSlot == null)
                return false;

            ToolIcon otherIcon = slot.toolInSlot;
            ChainSlot fromSlot = CurrentSlot;

            // Перемещаем другой инструмент в исходный слот
            otherIcon.PlaceInSlot(fromSlot);
        }

        PlaceInSlot(slot);
        return true;
    }

    private void PlaceInSlot(ChainSlot slot)
    {
        if (CurrentSlot != null)
        {
            CurrentSlot.toolInSlot = null;
            CurrentSlot.currentTool = ToolType.None;
        }

        transform.SetParent(slot.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        CurrentSlot = slot;
        slot.toolInSlot = this;
        slot.currentTool = toolType;
    }

    private ChainSlot GetNearestSlot()
    {
        float minDist = 40f; // Радиус автозахвата
        ChainSlot nearest = null;

        foreach (var slot in allSlots)
        {
            float dist = Vector3.Distance(rectTransform.position, slot.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = slot;
            }
        }

        return nearest;
    }

    public void ReturnToOriginalSlot()
    {
        InitializeOriginalSlotIfNeeded();

        if (CurrentSlot != null && CurrentSlot != originalParentSlot)
        {
            CurrentSlot.toolInSlot = null;
            CurrentSlot.currentTool = ToolType.None;
        }

        if (originalParentSlot != null)
        {
            transform.SetParent(originalParentSlot.transform, false);
            rectTransform.anchoredPosition = Vector2.zero;

            CurrentSlot = originalParentSlot;
            originalParentSlot.toolInSlot = this;
            originalParentSlot.currentTool = toolType;
        }
        else if (originalParent != null)
        {
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = Vector2.zero;
            CurrentSlot = originalParent.GetComponent<ChainSlot>();
            if (CurrentSlot != null)
            {
                CurrentSlot.toolInSlot = this;
                CurrentSlot.currentTool = toolType;
            }
        }

        iconImage.enabled = true;
        iconImage.sprite = iconSprite;
    }

    public void HandleDropOnSlot(ChainSlot slot)
    {
        dropHandledThisFrame = true;

        bool placed = TryPlaceInSlot(slot, allowSwap: true);
        if (!placed)
            ReturnToStartSlot();

        ClearHighlights();
        slot?.chainManager?.CheckChainComplete();
    }

    private void ReturnToStartSlot()
    {
        if (CurrentSlot != null)
        {
            CurrentSlot.toolInSlot = null;
            CurrentSlot.currentTool = ToolType.None;
        }

        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = originalAnchoredPosition;

        CurrentSlot = originalParent != null ? originalParent.GetComponent<ChainSlot>() : null;
        if (CurrentSlot != null)
        {
            CurrentSlot.toolInSlot = this;
            CurrentSlot.currentTool = toolType;
        }
    }

    private void ClearHighlights()
    {
        foreach (var slot in allSlots)
            slot.SetHighlight(false);
    }

}
