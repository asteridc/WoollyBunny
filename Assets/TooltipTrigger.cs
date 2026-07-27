using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Tooltip")]
    [SerializeField] private Tooltip tooltip;


    [Header("Overview")]
    [SerializeField] private bool canOpenOverview;
    [SerializeField] private WeaponOverview weaponOverview;
    [SerializeField] private WeaponOverviewData overviewData;


    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.Show();
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.Hide();
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canOpenOverview)
            return;


        if (eventData.button == PointerEventData.InputButton.Left)
        {
            weaponOverview.Show(overviewData);
        }
    }
}