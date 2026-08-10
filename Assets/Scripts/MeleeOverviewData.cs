using System;
using UnityEngine;

[Serializable]
public class MeleeOverviewData
{
    [Header("General")]
    public string weaponNameRu;
    public string weaponNameEn;
    public Sprite icon;

    [Header("Ability")]
    public Sprite iconAbility;
    public Tooltip tooltipAbility;

    public string typeRu;
    public string typeEn;

    [Header("Rarity")]
    public string rarityRu;
    public string rarityEn;
    public Color rarityColor = Color.white;

    [Header("Stats")]
    public int damage;

    [SerializeField] private int moduleCount = 2;
    public int ModuleCount => moduleCount;

    [Header("Description")]
    [TextArea(3, 6)]
    public string descriptionRu;

    [TextArea(3, 6)]
    public string descriptionEn;
}