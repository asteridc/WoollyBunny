using UnityEngine;

public class CombatUI : MonoBehaviour
{
    public GameObject attackPanel;
    public GameObject meleeButton;
    public GameObject rangedButton;

    public void ShowAttackOptions(CombatCharacter character)
    {
        attackPanel.SetActive(true);
    }

    public void HideAttackOptions()
    {
        attackPanel.SetActive(false);
    }

    public void OnChooseMelee()
    {
        CombatManager.Instance.OnAttackTypeSelected(AttackType.Melee);
    }

    public void OnChooseRanged()
    {
        CombatManager.Instance.OnAttackTypeSelected(AttackType.Ranged);
    }
}
