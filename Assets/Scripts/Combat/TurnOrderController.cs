using UnityEngine;
using System.Collections.Generic;

public class TurnOrderController : MonoBehaviour
{
    public List<CombatCharacter> order = new List<CombatCharacter>();

    public void BuildOrder(List<CombatCharacter> playerTeam, List<CombatCharacter> enemyTeam)
    {
        order.Clear();
        order.AddRange(playerTeam);
        order.AddRange(enemyTeam);
    }
}
