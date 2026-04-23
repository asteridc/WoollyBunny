using UnityEngine;

[CreateAssetMenu(menuName = "Localization/Tooltip Data")]
public class TooltipData : ScriptableObject
{
    [Header("Russian")]
    [TextArea] public string headingRu;
    [TextArea] public string textRu;

    [Header("English")]
    [TextArea] public string headingEn;
    [TextArea] public string textEn;

    public string GetHeading()
    {
        if (LanguageManager.CurrentLanguage == Language.English && !string.IsNullOrEmpty(headingEn))
            return headingEn;

        return headingRu;
    }

    public string GetText()
    {
        if (LanguageManager.CurrentLanguage == Language.English && !string.IsNullOrEmpty(textEn))
            return textEn;

        return textRu;
    }
}
