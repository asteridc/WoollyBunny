using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Perk")]
public class Perk : ScriptableObject
{
    public string perkId;
    public string title;
    [TextArea] public string description;
}
