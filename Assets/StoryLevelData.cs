using UnityEngine;

[System.Serializable]
public class StoryLevelData
{
    [Header("Level Info")]
    public int currentLevel = 1;
    public int storyExperience = 0;
    public int totalStoryExperience = 0;

    [Header("Experience Thresholds")]
    [SerializeField] private int[] experienceRequiredForLevel;

    public StoryLevelData()
    {
        currentLevel = 1;
        storyExperience = 0;
        totalStoryExperience = 0;

        // Опыт требуемый для каждого уровня
        // Уровень 1->2: 500, 2->3: 740, 3->4: 1020
        experienceRequiredForLevel = new int[]
        {
            0,      // Level 1 (начальный)
            500,    // Level 1->2
            740,    // Level 2->3
            1020,   // Level 3->4
            1360,   // Level 4->5 (прогрессия +340)
            1800,   // Level 5->6
            2300,   // Level 6->7
            2900,   // Level 7->8
            3600,   // Level 8->9
            4400,   // Level 9->10
            5300,   // Level 10->11
            6300,   // Level 11->12
            7400,   // Level 12->13
            8600,   // Level 13->14
            9900,   // Level 14->15
            11300,  // Level 15->16
            12800,  // Level 16->17
            14400,  // Level 17->18
            16100,  // Level 18->19
            18000,  // Level 19->20
        };
    }

    public int GetExperienceRequiredForNextLevel()
    {
        if (currentLevel >= 20)
            return 0;

        return experienceRequiredForLevel[currentLevel];
    }

    public int GetExperienceUntilNextLevel()
    {
        int required = GetExperienceRequiredForNextLevel();
        return required - storyExperience;
    }

    public float GetLevelProgressPercent()
    {
        if (currentLevel >= 20)
            return 1f;

        int required = GetExperienceRequiredForNextLevel();
        if (required <= 0)
            return 1f;

        return Mathf.Clamp01((float)storyExperience / required);
    }

    public void AddExperience(int amount)
    {
        storyExperience += amount;
        totalStoryExperience += amount;

        while (storyExperience >= GetExperienceRequiredForNextLevel() && currentLevel < 20)
        {
            storyExperience -= GetExperienceRequiredForNextLevel();
            currentLevel++;
        }
    }

    public void LevelUp()
    {
        if (currentLevel < 20)
        {
            storyExperience = 0;
            currentLevel++;
        }
    }
}
