using System;
using System.Collections.Generic;

[System.Serializable]
public class DialogueSaveData
{
    public string chapterId;
    public int chapterIndex;
    public int lineIndex;
    public string backgroundId;

    public int nobility;
    public int bloodthirst;
    public int love;

    public ChoiceSaveState choiceState;
}

[Serializable]
public class ChoiceSaveState
{
    public int lineIndex;
    public int choiceIndex;
    public int selectedIndex;
}

