using UnityEngine;

public class LocalizedStory : MonoBehaviour
{
    [SerializeField] private DialogueChapter russian;
    [SerializeField] private DialogueChapter english;

    public DialogueChapter GetFor(DialogueChapter fallback)
    {
        if (LanguageManager.CurrentLanguage == Language.English && english != null)
        {
            Debug.Log("Английский сюжет показан");
            return english;
        }
        return fallback != null ? fallback : russian;
    }
}