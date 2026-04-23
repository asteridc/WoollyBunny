using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WoollyBunny/Dialogue Chapter")]
public class DialogueChapter : ScriptableObject
{
    [SerializeField] private string chapterId;
    [Min(1)]
    [SerializeField] private int chapterNumber = 1;

    public List<DialogueLine> lines = new List<DialogueLine>();

    public string ChapterId
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(chapterId))
                return chapterId.Trim();

            return BuildFallbackChapterId();
        }
    }

    public int ChapterNumber => chapterNumber > 0 ? chapterNumber : 1;

    public bool MatchesChapterId(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return string.Equals(ChapterId, id.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private string BuildFallbackChapterId()
    {
        if (string.IsNullOrWhiteSpace(name))
            return string.Empty;

        const string englishSuffix = " eng";
        string fallbackId = name.Trim();

        if (fallbackId.EndsWith(englishSuffix, StringComparison.OrdinalIgnoreCase))
            fallbackId = fallbackId.Substring(0, fallbackId.Length - englishSuffix.Length);

        return fallbackId.ToLowerInvariant();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(chapterId))
            chapterId = chapterId.Trim();

        if (chapterNumber < 1)
            chapterNumber = 1;
    }
#endif
}
