using UnityEngine;
using TMPro;

[System.Serializable]
public class NotificationData
{
    public bool showCollectibleView = false;
    public string collectibleTitle;
    [TextArea(3, 10)] public string collectibleContent;
    public Sprite collectibleIcon;

    public bool showItemNotification;
    public string itemName;
    public string itemType;
    public ItemRarity itemRarity;
    public Sprite itemIcon;

    public bool showStoryNotification;
    [TextArea(2, 4)] public string storyNotificationText;
    public float storyNotificationDuration = 5.5f;
    public Sprite storyNotificationIcon;
    public Color storyNotificationTextColor = Color.white;
    public Color storyNotificationIconColor = Color.white;
    public float storyNotificationFontSize = 36f;

    public bool isBold;
    public bool isItalic;
    public bool isUppercase;
    public bool useCustomFont;
    public TMP_FontAsset customFont;
}
