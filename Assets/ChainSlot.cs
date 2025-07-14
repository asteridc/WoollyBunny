using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum ToolType
{
    None,
    Wire,
    Resistor,
    Battery,
    Switch,
    Capacitor
}

public class ChainSlot : MonoBehaviour, IDropHandler
{

    public ToolIcon toolInSlot;
    public Image highlightImage;
    public bool isChainSlot = false;

    public ElectroChainManager chainManager;

    void Awake()
    {
        if (highlightImage != null)
            highlightImage.enabled = false;
    }

    void Start()
    {
        ToolIcon.allSlots.Add(this);
        if (highlightImage != null)
            highlightImage.enabled = false;
    }

    public ToolType currentTool = ToolType.None;
    public Image toolIconImage; // Иконка инструмента в этом слоте

    public void SetTool(ToolType tool, Sprite icon)
    {
        currentTool = tool;
        toolIconImage.sprite = icon;
        toolIconImage.enabled = tool != ToolType.None;
    }

    public void ClearTool()
    {
        currentTool = ToolType.None;
        toolInSlot = null;
    }


    public void OnDrop(PointerEventData eventData)
    {
        var draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        var dragTool = draggedObj.GetComponent<ToolIcon>();
        if (dragTool == null) return;

        // Если этот слот уже занят — убираем старый инструмент
        if (toolInSlot != null)
        {
            ToolIcon oldTool = toolInSlot;

            // Убираем ссылки
            oldTool.CurrentSlot = null;
            toolInSlot = null;
            currentTool = ToolType.None;

            // Возвращаем старый инструмент в ящик
            oldTool.transform.SetParent(oldTool.originalParent, false);
            oldTool.rectTransform.anchoredPosition = Vector2.zero;
        }

        // Освобождаем предыдущий слот нового инструмента
        if (dragTool.CurrentSlot != null)
        {
            dragTool.CurrentSlot.toolInSlot = null;
            dragTool.CurrentSlot.currentTool = ToolType.None;
            dragTool.CurrentSlot = null;
        }

        // Обновляем ссылки
        dragTool.CurrentSlot = this;
        toolInSlot = dragTool;
        currentTool = dragTool.toolType;

        // Перемещаем иконку
        dragTool.transform.SetParent(transform, false);
        dragTool.rectTransform.anchoredPosition = Vector2.zero;

        // Убираем подсветку со всех слотов
        foreach (var slot in ToolIcon.allSlots)
            slot.SetHighlight(false);

        // Проверка цепи
        chainManager?.CheckChainComplete();
    }

    public void SetHighlight(bool show)
    {
        if (highlightImage != null)
            highlightImage.enabled = show;
    }

    public bool CanAccept(ToolIcon tool)
    {
        return toolInSlot == null;
    }
}