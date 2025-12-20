using UnityEngine;
using UnityEngine.EventSystems;

public class SettingItemHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea] public string tooltipDescription;
    [TextArea] public string tooltipHeading;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipManagerInstance() != null)
            TooltipManagerInstance().ShowTooltip(tooltipHeading, tooltipDescription, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipManagerInstance() != null)
            TooltipManagerInstance().HideTooltip();
    }

    private TooltipManager TooltipManagerInstance()
    {
        return FindObjectOfType<TooltipManager>(); // или найди через FindObjectOfType<TooltipManager>()
    }
}
