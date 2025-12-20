using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Weapon")]
public class Weapon : ScriptableObject
{
    public string weaponId;
    public string displayName;
    public int damage = 10;
    public float hitChance = 0.9f;
    public bool isRanged = false;
}
