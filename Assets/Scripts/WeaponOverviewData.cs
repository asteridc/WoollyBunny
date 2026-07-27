using System;
using UnityEngine;

[Serializable]
public class WeaponOverviewData
{
    [Header("General")]
    public string weaponNameRu;
    public string weaponNameEn;
    public Sprite icon;
    public string typeRu;
    public string typeEn;

    [Header("Rarity")]
    public string rarityRu;
    public string rarityEn;
    public Color rarityColor = Color.white;

    [Header("Stats")]
    public int damage;

    [Range(0f, 10f)]
    public float headMultiplier = 2f;

    [Range(0f, 10f)]
    public float torsoMultiplier = 1f;

    [Range(0f, 10f)]
    public float armsMultiplier = 0.8f;

    [Range(0f, 10f)]
    public float legsMultiplier = 0.7f;

    [Header("Description")]
    [TextArea(3, 6)]
    public string descriptionRu;
    [TextArea(3, 6)]
    public string descriptionEn;
}