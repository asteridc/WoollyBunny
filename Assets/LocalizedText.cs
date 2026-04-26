using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    [TextArea] public string russianText;
    [TextArea] public string englishText;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        LanguageManager.OnLanguageChanged += UpdateText;
        UpdateText(LanguageManager.CurrentLanguage);
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= UpdateText;
    }

    private void UpdateText(Language language)
    {
        text.text = language == Language.English ? englishText : russianText;
    }
}
