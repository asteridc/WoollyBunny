using UnityEngine;

[CreateAssetMenu(
    fileName = "CollectibleTextItem",
    menuName = "Localization/Collectible Item/Text Item (Note / Document)"
)]
public class CollectibleTextItem : CollectibleItemBase
{
    [Header("Icon")]
    public Sprite icon;

    [Header("Russian")]
    [TextArea]
    public string titleRu;

    [TextArea(5, 25)]
    public string contentRu;

    [Header("English")]
    [TextArea]
    public string titleEn;

    [TextArea(5, 25)]
    public string contentEn;

    public override Sprite GetIcon()
    {
        return icon;
    }

    public override string GetTitle()
    {
        if (
            LanguageManager.CurrentLanguage == Language.English &&
            !string.IsNullOrEmpty(titleEn)
        )
        {
            return titleEn;
        }

        return titleRu;
    }

    public override string GetContent()
    {
        if (
            LanguageManager.CurrentLanguage == Language.English &&
            !string.IsNullOrEmpty(contentEn)
        )
        {
            return contentEn;
        }

        return contentRu;
    }
}