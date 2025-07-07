using UnityEngine;

public enum ChoiceType { Optional, Required }
public enum DominantPath { None, Bloodthirst, Nobility, Love }

[System.Serializable]
public class DialogueLine
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

    // ====== ÌÎÄÅËÜÊÀ ÑËÓØÀŞÙÅÃÎ ======
    public string listenerCharacterName;
    public bool isListenerOnRight = false;
    public bool flipListenerImage = false;

    // ====== ÏÎÊÀÇ ÊÎËËÅÊÖÈÎÍÍÎÃÎ ÏĞÅÄÌÅÒÀ (ÍÅ ×ÅĞÅÇ ÈÍÂÅÍÒÀĞÜ) ======
    public bool showCollectibleView = false;
    public string collectibleTitle;
    [TextArea(3, 10)] public string collectibleContent;
    public Sprite collectibleIcon;

    // ====== ÏĞÅÄÌÅÒÛ (óâåäîìëåíèå î êîëëåêöèîíêå) ======
    public bool showItemNotification;
    public string itemName;
    public string itemType;
    public ItemRarity itemRarity;
    public Sprite itemIcon;

    // ====== ÂÛÁÎĞÛ ======
    public bool isJumpLine = false;
    public int gotoLineIndex;
    public bool hasChoices;

    // ====== ÏÎÑËÅÄÑÒÂÈß (ÂÛÁÎĞÛ Ñ ÏÓÒßÌÈ) ======
    public bool hasPathConsequences;

    [System.Serializable]
    public class PathVarients
    {
        public DominantPath path;
        public string overrideText;
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