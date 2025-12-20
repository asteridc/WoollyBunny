using UnityEngine;

public enum CombatState
{
    Start,
    PlayerChooseAttacker,
    PlayerChooseAttackType,
    PlayerChooseTarget,
    EnemyTurn,
    End
}

public enum AttackType
{
    Melee,
    Ranged
}
