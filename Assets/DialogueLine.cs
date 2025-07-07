using UnityEngine;
using TMPro;

public enum ChoiceType { Optional, Required }
public enum DominantPath { None, Bloodthirst, Nobility, Love }

[System.Serializable]
public class DialogueLine : IStoryNotificationSettings
{
    [TextArea(2, 5)]
    public string text;
    public string index;

    public string speakerName;
    public string characterName;

    public bool changeSpeakerName = true;
    public bool changeCharacterSprite = false;
    public bool changeBackground = false;

    public Sprite characterSprite;
    public Sprite backgroundSprite;

    public bool isOnRight = false;
    public bool flipSpeakerImage = false;

    // ====== ÌÎÄÅËÜÊÀ ÑËÓØÀÞÙÅÃÎ ======
    public string listenerCharacterName;
    public bool isListenerOnRight = false;
    public bool flipListenerImage = false;

    // ====== ÏÎÊÀÇ ÊÎËËÅÊÖÈÎÍÍÎÃÎ ÏÐÅÄÌÅÒÀ (ÍÅ ×ÅÐÅÇ ÈÍÂÅÍÒÀÐÜ) ======
    public bool showCollectibleView = false;
    public string collectibleTitle;
    [TextArea(3, 10)] public string collectibleContent;
    public Sprite collectibleIcon;

    // ====== ÏÐÅÄÌÅÒÛ (óâåäîìëåíèå î êîëëåêöèîíêå) ======
    public bool showItemNotification;
    public string itemName;
    public string itemType;
    public ItemRarity itemRarity;
    public Sprite itemIcon;

    [Header("Story Notification")]
    public bool showStoryNotification;
    [TextArea(2,4)] public string storyNotificationText;
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

    // Ðåàëèçàöèÿ èíòåðôåéñà
    string IStoryNotificationSettings.storyNotificationText => storyNotificationText;
    float IStoryNotificationSettings.storyNotificationDuration => storyNotificationDuration;
    Sprite IStoryNotificationSettings.storyNotificationIcon => storyNotificationIcon;
    Color IStoryNotificationSettings.storyNotificationTextColor => storyNotificationTextColor;
    Color IStoryNotificationSettings.storyNotificationIconColor => storyNotificationIconColor;
    float IStoryNotificationSettings.storyNotificationFontSize => storyNotificationFontSize;

    bool IStoryNotificationSettings.isBold => isBold;
    bool IStoryNotificationSettings.isItalic => isItalic;
    bool IStoryNotificationSettings.isUppercase => isUppercase;
    bool IStoryNotificationSettings.useCustomFont => useCustomFont;
    TMP_FontAsset IStoryNotificationSettings.customFont => customFont;

    // ====== ÂÛÁÎÐÛ ======
    public bool isJumpLine = false;
    public int gotoLineIndex;
    public bool hasChoices;

    // ====== ÏÎÑËÅÄÑÒÂÈß (ÂÛÁÎÐÛ Ñ ÏÓÒßÌÈ) ======
    public bool hasPathConsequences;
    [System.Serializable]
    public class PathVarients : IStoryNotificationSettings
    {
        public DominantPath path;
        public string overrideText;

        public bool showStoryNotification;
        public string storyNotificationText;
        public Sprite storyNotificationIcon;
        public float storyNotificationDuration = 5.5f;
        public Color storyNotificationTextColor = Color.white;
        public Color storyNotificationIconColor = Color.white;
        public float storyNotificationFontSize = 36f;

        public bool useCustomFont;
        public TMP_FontAsset customFont;

        public bool isBold;
        public bool isItalic;
        public bool isUppercase;

        // Ðåàëèçàöèÿ èíòåðôåéñà
        string IStoryNotificationSettings.storyNotificationText => storyNotificationText;
        float IStoryNotificationSettings.storyNotificationDuration => storyNotificationDuration;
        Sprite IStoryNotificationSettings.storyNotificationIcon => storyNotificationIcon;
        Color IStoryNotificationSettings.storyNotificationTextColor => storyNotificationTextColor;
        Color IStoryNotificationSettings.storyNotificationIconColor => storyNotificationIconColor;
        float IStoryNotificationSettings.storyNotificationFontSize => storyNotificationFontSize;

        bool IStoryNotificationSettings.isBold => isBold;
        bool IStoryNotificationSettings.isItalic => isItalic;
        bool IStoryNotificationSettings.isUppercase => isUppercase;
        bool IStoryNotificationSettings.useCustomFont => useCustomFont;
        TMP_FontAsset IStoryNotificationSettings.customFont => customFont;
    }

    public PathVarients[] pathVarients;

    [System.Serializable]
    public class Choice
    {
        public string choiceText;
        public int pathPointsBloodthirsty;
        public int pathPointsNoble;
        public int pathPointsLove;
        public DominantPath pathReward = DominantPath.None;
        public int nextLineIndex;
        public ChoiceType choiceType;
    }

    public Choice[] choices;

    [System.Serializable]
    public class ExtraActions
    {
        public bool showCodePanel = false;
        public bool showNotePanel = false;
        public bool stopDialogueAfterThisLine = false;
        public GameObject objectToActivate;
    }

    public ExtraActions extraActions;
}