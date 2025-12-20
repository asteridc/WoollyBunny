using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CombatCharacter : MonoBehaviour
{
    [Header("Основные данные")]
    public string characterName;
    public bool isPlayer = true;

    [Header("Статы")]
    public int health = 100;
    public int maxHealth = 100;
    public int meleeDamage = 20;
    public int rangedDamage = 15;

    [Header("Ссылка на визуал персонажа")]
    public GameObject view;

    [Header("Перки и оружие")]
    public List<Perk> perks = new List<Perk>();
    public Weapon meleeWeapon;
    public Weapon rangedWeapon;

    [Header("UI персонажа")]
    public GameObject uiPanel; // панель с HP и именем
    public Text nameText;
    public Image healthBar;

    // ============================
    // Жив ли персонаж
    // ============================
    public bool IsAlive()
    {
        return health > 0;
    }

    // ============================
    // Обновление UI
    // ============================
    public void UpdateUI()
    {
        if (uiPanel != null)
        {
            if (nameText != null)
                nameText.text = characterName;

            if (healthBar != null)
                healthBar.fillAmount = (float)health / maxHealth;
        }
    }

    // ============================
    // Получение урона
    // ============================
    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health < 0) health = 0;

        UpdateUI();

        // можно здесь вызвать анимацию урона
        PlayHitAnimation();
    }

    // ============================
    // Метод для атаки
    // ============================
    public int GetDamage(AttackType type)
    {
        switch (type)
        {
            case AttackType.Melee:
                return meleeDamage + (meleeWeapon != null ? meleeWeapon.damage : 0);
            case AttackType.Ranged:
                return rangedDamage + (rangedWeapon != null ? rangedWeapon.damage : 0);
            default:
                return 0;
        }
    }

    // ============================
    // Анимации (пока заглушки)
    // ============================
    public void PlayAttackAnimation()
    {
        Debug.Log(characterName + " атакует.");
        // сюда подключим аниматор позже
    }

    public void PlayHitAnimation()
    {
        Debug.Log(characterName + " получил урон.");
    }
}
