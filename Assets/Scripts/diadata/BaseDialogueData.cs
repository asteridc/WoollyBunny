using UnityEngine;
using TMPro;

[System.Serializable]
public class BaseDialogueData
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

    public bool isOnRight = false;
    public bool flipSpeakerImage = false;

    public string listenerCharacterName;
    public bool isListenerOnRight = false;
    public bool flipListenerImage = false;
}