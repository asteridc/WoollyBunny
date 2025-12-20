using UnityEngine;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Персонажи игрока")]
    public CombatCharacter[] playerCharacters;

    [Header("Персонажи врага")]
    public CombatCharacter[] enemyCharacters;

    [Header("UI боя")]
    public CombatUI combatUI;

    [HideInInspector] public CombatCharacter attacker;
    [HideInInspector] public CombatCharacter target;
    [HideInInspector] public AttackType chosenAttackType;

    public CombatState state;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        StartBattle();
    }

    public void StartBattle()
    {
        state = CombatState.PlayerChooseAttacker;
        combatUI.HideAttackOptions();
        Debug.Log("Бой начался. Игрок выбирает атакующего.");
    }

    // ======================
    //  ОБРАБОТКА КЛИКОВ
    // ======================
    public void OnCharacterClicked(CombatCharacter clicked)
    {
        switch (state)
        {
            case CombatState.PlayerChooseAttacker:
                if (clicked.isPlayer)
                {
                    attacker = clicked;
                    Debug.Log("Игрок выбрал атакующего: " + attacker.characterName);

                    state = CombatState.PlayerChooseAttackType;
                    combatUI.ShowAttackOptions(attacker);
                }
                break;

            case CombatState.PlayerChooseTarget:
                if (!clicked.isPlayer)
                {
                    target = clicked;
                    Debug.Log("Игрок выбрал цель: " + target.characterName);

                    ResolveAttack(attacker, target, chosenAttackType);
                }
                break;
        }
    }

    // ======================
    //  ВЫБОР ТИПА АТАКИ
    // ======================
    public void OnAttackTypeSelected(AttackType type)
    {
        chosenAttackType = type;
        combatUI.HideAttackOptions();

        state = CombatState.PlayerChooseTarget;
        Debug.Log("Игрок выбрал атаку: " + type + ". Теперь выбери цель.");
    }

    // ======================
    //  ПРОВЕДЕНИЕ АТАКИ
    // ======================
    private void ResolveAttack(CombatCharacter attacker, CombatCharacter target, AttackType type)
    {
        int dmg = 0;

        switch (type)
        {
            case AttackType.Melee:
                dmg = attacker.meleeDamage;
                break;

            case AttackType.Ranged:
                dmg = attacker.rangedDamage;
                break;
        }

        target.health -= dmg;

        Debug.Log(attacker.characterName + " наносит " + dmg + " урона → " + target.characterName);

        if (target.health <= 0)
        {
            Debug.Log(target.characterName + " погиб.");
        }

        StartEnemyTurn();
    }

    // ======================
    //  ХОД ВРАГА (простая заглушка)
    // ======================
    private void StartEnemyTurn()
    {
        state = CombatState.EnemyTurn;
        Debug.Log("Ход врага...");

        // потом сделаем ИИ
        EndEnemyTurn();
    }

    private void EndEnemyTurn()
    {
        state = CombatState.PlayerChooseAttacker;
        Debug.Log("Ход снова у игрока. Выбери атакующего.");
    }
}
