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

    public static List<ChainSlot> allSlots = new List<ChainSlot>();

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (originalParent == null)
            originalParent = transform.parent;

        if (originalParentSlot == null)
            originalParentSlot = GetComponentInParent<ChainSlot>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
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

        ChainSlot targetSlot = GetNearestSlot();
        bool success = false;

        if (targetSlot != null)
        {
            // Если слот пуст — просто вставляем
            if (targetSlot.toolInSlot == null)
            {
                MoveToSlot(targetSlot);
                success = true;
            }
            else
            {
                // Обмен
                ToolIcon otherIcon = targetSlot.toolInSlot;
                ChainSlot fromSlot = CurrentSlot;

                // Обмен позиций
                if (otherIcon == null)
                {
                    Debug.LogError("otherIcon is null в OnEndDrag");
                }
                else if (fromSlot == null)
                {
                    Debug.LogError("fromSlot is null в OnEndDrag");
                }
                else
                {
                    otherIcon.transform.SetParent(fromSlot.transform);
                }

                otherIcon.rectTransform.position = fromSlot.transform.position;
                otherIcon.CurrentSlot = fromSlot;
                fromSlot.toolInSlot = otherIcon;

                // Переместить текущую
                MoveToSlot(targetSlot);
                success = true;
            }
        }

        if (!success)
        {
            // Возврат назад
            transform.SetParent(originalParent, false);
            rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        foreach (var slot in allSlots)
            slot.SetHighlight(false);
    }

    private void MoveToSlot(ChainSlot slot)
    {
        if (CurrentSlot != null)
            CurrentSlot.toolInSlot = null;

        transform.SetParent(slot.transform, false);
        rectTransform.anchoredPosition = Vector2.zero;

        CurrentSlot = slot;
        slot.toolInSlot = this;
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
        transform.SetParent(originalParent, false);
        rectTransform.anchoredPosition = Vector2.zero;

        CurrentSlot = originalParentSlot;
        if (originalParentSlot != null)
            originalParentSlot.toolInSlot = this;

        iconImage.enabled = true;
        iconImage.sprite = iconSprite;
    }

}