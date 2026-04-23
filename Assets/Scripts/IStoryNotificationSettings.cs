using TMPro;
using UnityEngine;

public interface IStoryNotificationSettings
{
    string storyNotificationText { get; }
    float storyNotificationDuration { get; }
    Sprite storyNotificationIcon { get; }
    Color storyNotificationTextColor { get; }
    Color storyNotificationIconColor { get; }
    float storyNotificationFontSize { get; }

    bool isBold { get; }
    bool isItalic { get; }
    bool isUppercase { get; }
    bool useCustomFont { get; }
    TMP_FontAsset customFont { get; }
}