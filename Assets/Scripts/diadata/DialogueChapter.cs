using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WoollyBunny/Dialogue Chapter")]
public class DialogueChapter : ScriptableObject
{
    [Header("Stable Chapter Identity")]
    [Tooltip("Stable ID shared across all localizations of the same chapter.")]
    public string chapterId;

    [Tooltip("Stable 0-based chapter index used for saves and UI. Set to -1 to fallback to list order.")]
    public int chapterIndex = -1;

    public List<DialogueLine> lines = new List<DialogueLine>();

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(chapterId))
            chapterId = name;
    }
}
