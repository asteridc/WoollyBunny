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

    [SerializeField] private TMPro.TextMeshProUGUI damageText;


    public void OnPointerEnter(PointerEventData eventData)
    {
        UpdateDamageDisplay();
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

    private void UpdateDamageDisplay()
    {
        if (damageText == null)
            return;


        // Дальнобойное оружие
        if (canOpenWeaponOverview && overviewData != null)
        {
            int damage = overviewData.damage;

            if (AccountManager.Instance != null)
            {
                damage =
                    AccountManager.Instance.GetScaledWeaponDamage(damage);
            }

            damageText.text = damage.ToString();
            return;
        }


        // Холодное оружие
        if (canOpenMeleeOverview && meleeOverviewData != null)
        {
            int damage = meleeOverviewData.damage;

            if (AccountManager.Instance != null)
            {
                damage =
                    AccountManager.Instance.GetScaledWeaponDamage(damage);
            }

            damageText.text = damage.ToString();
            return;
        }


        // Если это не оружие
        damageText.text = "";
    }

    public void SetTooltip(Tooltip tooltip)
    {
        this.tooltip = tooltip;
    }
}