using System;
using UnityEngine;

public enum DailyQuestRewardType
{
    StoryExperience,
    BloodthirstPoints,
    NoblePoints,
    LovePoints,
    Custom,
    InventoryEntry
}

[Serializable]
public class DailyQuestDefinition
{
    public string id;
    public string actionId;
    public string chapterId;
    public int chapterNumber;
    [Tooltip("Номер строки в Inspector, начиная с 1. Значение -1 отключает привязку.")]
    public int dialogueLineIndex = -1;
    [TextArea] public string russianDescription;
    [TextArea] public string englishDescription;
    [Min(1)] public int requiredCount = 1;
    public DailyQuestRewardType rewardType;
    [Min(0)] public int rewardAmount = 1;
    public string customRewardId;
    public string russianRewardName;
    public string englishRewardName;
    public string inventoryEntryId;
}
