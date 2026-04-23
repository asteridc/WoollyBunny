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

    void OnEnable()
    {
        if (!ToolIcon.allSlots.Contains(this))
            ToolIcon.allSlots.Add(this);

        RefreshStateFromHierarchy();

        if (highlightImage != null)
            highlightImage.enabled = false;
    }

    void OnDisable()
    {
        ToolIcon.allSlots.Remove(this);
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

    public void RefreshStateFromHierarchy()
    {
        ToolIcon childTool = GetComponentInChildren<ToolIcon>(true);

        if (childTool == null || childTool.gameObject == gameObject)
        {
            ClearTool();
            return;
        }

        toolInSlot = childTool;
        currentTool = childTool.toolType;
        childTool.CurrentSlot = this;
    }


    public void OnDrop(PointerEventData eventData)
    {
        var draggedObj = eventData.pointerDrag;
        if (draggedObj == null) return;

        var dragTool = draggedObj.GetComponent<ToolIcon>();
        if (dragTool == null) return;

        dragTool.HandleDropOnSlot(this);
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
