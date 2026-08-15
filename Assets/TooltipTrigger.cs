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

    [Header("Collectible")]
    [SerializeField] private bool canOpenCollectibleOverview;
    [SerializeField] private CollectibleTooltip collectibleTooltip;

    [SerializeField] private CollectibleTextItem collectibleItem;

    [Tooltip("Готовый визуальный объект рамки этого коллекционного предмета.")]
    [SerializeField] private GameObject collectibleItemContainer;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (canOpenCollectibleOverview)
        {
            if (collectibleTooltip != null &&
                collectibleItem != null)
            {
                collectibleTooltip.Show(
                    collectibleItem.icon,
                    collectibleItem.GetTitle(),
                    collectibleItem.GetContent()
                );
            }

            return;
        }

        UpdateDamageDisplay();

        if (tooltip != null)
            tooltip.Show();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (canOpenCollectibleOverview)
        {
            if (collectibleTooltip != null)
                collectibleTooltip.Hide();

            return;
        }

        if (tooltip != null)
            tooltip.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // =========================================================
        // COLLECTIBLE
        // =========================================================

        if (canOpenCollectibleOverview)
        {
            if (collectibleTooltip != null)
                collectibleTooltip.Hide();

            if (BackpackCollectiblesPanel.Instance == null)
            {
                Debug.LogError(
                    "TooltipTrigger: BackpackCollectiblesPanel.Instance is null.",
                    this
                );

                return;
            }

            BackpackCollectiblesPanel.Instance.OpenCollectibleOverview(
                collectibleItem,
                collectibleItemContainer
            );

            return;
        }

        // =========================================================
        // WEAPON
        // =========================================================

        if (canOpenWeaponOverview)
        {
            if (weaponOverview != null)
                weaponOverview.Show(overviewData);

            return;
        }

        // =========================================================
        // MELEE
        // =========================================================

        if (canOpenMeleeOverview)
        {
            if (meleeOverview != null)
                meleeOverview.Show(meleeOverviewData);
        }
    }

    private void UpdateDamageDisplay()
    {
        if (damageText == null)
            return;

        if (canOpenWeaponOverview &&
            overviewData != null)
        {
            int damage = overviewData.damage;

            if (AccountManager.Instance != null)
            {
                damage =
                    AccountManager.Instance
                        .GetScaledWeaponDamage(damage);
            }

            damageText.text = damage.ToString();
            return;
        }

        if (canOpenMeleeOverview &&
            meleeOverviewData != null)
        {
            int damage = meleeOverviewData.damage;

            if (AccountManager.Instance != null)
            {
                damage =
                    AccountManager.Instance
                        .GetScaledWeaponDamage(damage);
            }

            damageText.text = damage.ToString();
            return;
        }

        damageText.text = "";
    }

    public void SetTooltip(Tooltip tooltip)
    {
        this.tooltip = tooltip;
    }
}