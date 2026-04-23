using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [TextArea] public string russianText;
    [TextArea] public string englishText;

    private TextMeshProUGUI text;

    void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        LanguageManager.OnLanguageChanged += UpdateText;
        UpdateText(LanguageManager.CurrentLanguage);
    }

    void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText(Language language)
    {
        text.text = language == Language.English ? englishText : russianText;
    }
}