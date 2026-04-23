using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class GameSaveData
{
    public string sceneName;
    public DialogueSaveData dialogueData;
    public string saveTime;
    public string saveLanguage;

    // --- мнбне ---
    public List<LoopSaveState> loops = new List<LoopSaveState>();
    public bool electroMinigameActive;
    public bool guitarMinigameActive;
    public Image backgroundImage; 
}

[Serializable]
public class LoopSaveState
{
    public string id;
    public bool active;
}

