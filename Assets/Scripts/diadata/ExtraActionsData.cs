using UnityEngine;

[System.Serializable]
public class ExtraActionsData
{
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
