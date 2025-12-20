using UnityEngine;

[System.Serializable]
public class ChoiceData
{
    public bool hasChoices;
    public bool isJumpLine = false;
    public int gotoLineIndex;

    public bool useFallbackIfNoRequired = false;
    public int fallbackLineIndex = -1;

    public DialogueLine.Choice[] choices;
}