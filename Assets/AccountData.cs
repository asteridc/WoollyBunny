using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AccountData
{
    [Header("Story Progress")]
    public StoryLevelData storyLevelData = new StoryLevelData();

    [Header("Chapter Experience Rewards")]
    public List<int> rewardedChapterNumbers = new List<int>();

    [Header("Account Inventory")]
    public List<AccountInventoryAmount> inventoryAmounts = new List<AccountInventoryAmount>();
    public List<string> ownedUniqueInventoryItems = new List<string>();
    public bool inventoryInitialized;
    public string dailyQuestProgressJson;

    [Header("Halloween Event")]
    public long pumpkinLanternLastAccrualUnixSeconds;

    public AccountData()
    {
        storyLevelData = new StoryLevelData();
        rewardedChapterNumbers = new List<int>();
    }
}