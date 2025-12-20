using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WoollyBunny/Dialogue Chapter")]
public class DialogueChapter : ScriptableObject
{
    public List<DialogueLine> lines = new List<DialogueLine>();
}