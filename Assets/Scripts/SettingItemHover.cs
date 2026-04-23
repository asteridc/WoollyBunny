using UnityEngine;
using UnityEngine.EventSystems;

public class SettingItemHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private TooltipData tooltipData;

    public void OnPointerEnter(PointerEventData eventData)
    {
        var tooltipManager = TooltipManagerInstance();
        if (tooltipManager == null || tooltipData == null) return;

        tooltipManager.ShowTooltip(
            tooltipData,
            transform.position
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var tooltipManager = TooltipManagerInstance();
        if (tooltipManager == null) return;

        tooltipManager.HideTooltip();
    }

    private TooltipManager TooltipManagerInstance()
    {
        return FindObjectOfType<TooltipManager>();
    }
}
