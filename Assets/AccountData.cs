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

    public AccountData()
    {
        storyLevelData = new StoryLevelData();
        rewardedChapterNumbers = new List<int>();
    }
}