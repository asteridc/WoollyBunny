using UnityEngine;
using TMPro;

public enum ChoiceType { Optional, Required }
public enum DominantPath { None, Bloodthirst, Nobility, Love }

[System.Serializable]
public class DialogueLine : IStoryNotificationSettings
{
    [TextArea(2, 5)]
    public string text;

    public string speakerName;
    public string characterName;

    public bool isRadio = false;
    public bool changeSpeakerName = true;
    public bool changeCharacterSprite = false;
    public bool changeBackground = false;

    public Sprite characterSprite;
    public Sprite backgroundSprite;
    public string backgroundId;

    public bool isOnRight = false;
    public bool flipSpeakerImage = false;

    // ====== ÌÎÄÅËÜÊÀ ÑËÓØÀÞÙÅÃÎ ======
    public string listenerCharacterName;
    public bool isListenerOnRight = false;
    public bool flipListenerImage = false;

    // ====== ÏÎÊÀÇ ÊÎËËÅÊÖÈÎÍÍÎÃÎ ÏÐÅÄÌÅÒÀ ======
    public bool showCollectibleView = false;
    public string collectibleTitle;
    [TextArea(3, 10)] public string collectibleContent;
    public Sprite collectibleIcon;

    // ====== ÏÐÅÄÌÅÒÛ ======
    public bool showItemNotification;
    public string itemName;
    public string itemType;
    public ItemRarity itemRarity;
    public Sprite itemIcon;

    [Header("Story Notification")]
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

    [Header("Unlock Keys")]
    public string[] unlockChoiceKeys;


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

    // ====== ÏÎÑËÅÄÑÒÂÈß (ÏÓÒÈ) ======
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

    public bool isEndOfChapter = false;

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
        public bool isSpecialChoice;

        [Header("Choice Lock")]
        public bool isLocked;
        public bool isAvailable => !isLocked;
        public string unlockKey;
        public Sprite lockImage;        
    }

    public bool useFallbackIfNoRequired = false;
    public int fallbackLineIndex = -1;

    public Choice[] choices;

    [System.Serializable]
    public class ExtraActions
    {
        [Header("Choice Hints")]
        public bool isImportantStoryChoice = false;

        [Header("Code Panel")]
        public bool showCodePanel = false;
        public bool showNotePanel = false;
        public bool stopDialogueAfterThisLine = false;
        public GameObject objectToActivate;

        [Header("Electro Substation")]
        public bool showElectroSubstationMinigame;
        public bool showInteractionPoints = false;

        [Header("The Final Line")]
        public bool isEndOfChapter = false;
    }

    public ExtraActions extraActions;

    public DialogueLine Clone()
    {
        // Shallow copy for primitive fields
        DialogueLine clone = (DialogueLine)this.MemberwiseClone();

        // Clone string array
        if (unlockChoiceKeys != null)
            clone.unlockChoiceKeys = (string[])unlockChoiceKeys.Clone();

        // Clone pathVarients
        if (pathVarients != null)
        {
            clone.pathVarients = new PathVarients[pathVarients.Length];
            for (int i = 0; i < pathVarients.Length; i++)
            {
                var src = pathVarients[i];
                var dst = new PathVarients
                {
                    path = src.path,
                    overrideText = src.overrideText,

                    showStoryNotification = src.showStoryNotification,
                    storyNotificationText = src.storyNotificationText,
                    storyNotificationIcon = src.storyNotificationIcon,
                    storyNotificationDuration = src.storyNotificationDuration,
                    storyNotificationTextColor = src.storyNotificationTextColor,
                    storyNotificationIconColor = src.storyNotificationIconColor,
                    storyNotificationFontSize = src.storyNotificationFontSize,

                    useCustomFont = src.useCustomFont,
                    customFont = src.customFont,

                    isBold = src.isBold,
                    isItalic = src.isItalic,
                    isUppercase = src.isUppercase
                };

                clone.pathVarients[i] = dst;
            }
        }

        // Clone choices
        if (choices != null)
        {
            clone.choices = new Choice[choices.Length];
            for (int i = 0; i < choices.Length; i++)
            {
                var c = choices[i];
                clone.choices[i] = new Choice
                {
                    choiceText = c.choiceText,
                    pathPointsBloodthirsty = c.pathPointsBloodthirsty,
                    pathPointsNoble = c.pathPointsNoble,
                    pathPointsLove = c.pathPointsLove,
                    pathReward = c.pathReward,
                    nextLineIndex = c.nextLineIndex,
                    choiceType = c.choiceType,
                    isSpecialChoice = c.isSpecialChoice,

                    isLocked = c.isLocked,
                    // isAvailable is a derived property, don't assign
                    unlockKey = c.unlockKey,
                    lockImage = c.lockImage
                };
            }
        }

        // Clone ExtraActions (single object, not array)
        if (extraActions != null)
        {
            clone.extraActions = new ExtraActions
            {
                isImportantStoryChoice = extraActions.isImportantStoryChoice,
                showCodePanel = extraActions.showCodePanel,
                showNotePanel = extraActions.showNotePanel,
                stopDialogueAfterThisLine = extraActions.stopDialogueAfterThisLine,
                objectToActivate = extraActions.objectToActivate,
                showElectroSubstationMinigame = extraActions.showElectroSubstationMinigame,
                showInteractionPoints = extraActions.showInteractionPoints,
                isEndOfChapter = extraActions.isEndOfChapter
            };
        }

        return clone;
    }
}