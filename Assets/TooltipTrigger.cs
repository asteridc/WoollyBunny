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
    [SerializeField] private bool canOpenWeaponOverview;
    [SerializeField] private bool canOpenMeleeOverview;
    [SerializeField] private WeaponOverview weaponOverview;
    [SerializeField] private WeaponOverviewData overviewData;
    [SerializeField] private MeleeOverview meleeOverview;
    [SerializeField] private MeleeOverviewData meleeOverviewData;


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
        if (!canOpenWeaponOverview && !canOpenMeleeOverview)
            return;



        if (eventData.button == PointerEventData.InputButton.Left && canOpenWeaponOverview)
        {
            weaponOverview.Show(overviewData);
        }

        if (eventData.button == PointerEventData.InputButton.Left && canOpenMeleeOverview)
        {
            meleeOverview.Show(meleeOverviewData);
        }

    }
}