using System;
using UnityEngine;

[Serializable]
public class AccountData
{
    [Header("Story Progress")]
    public StoryLevelData storyLevelData = new StoryLevelData();

    public AccountData()
    {
        storyLevelData = new StoryLevelData();
    }
}
