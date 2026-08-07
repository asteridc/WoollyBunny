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
            740,    // Level 2->3 (+240)
            1020,   // Level 3->4 (прогрессия +280)
            1360,   // Level 4->5 (прогрессия +340)
            1780,   // Level 5->6 (прогрессия +420)
            2400,   // Level 6->7 (прогрессия +520)
            3040,   // Level 7->8 (прогрессия +640)
            3820,   // Level 8->9 (прогрессия +780)
            4760,   // Level 9->10 (прогрессия +940)
            5880,   // Level 10->11 (+1120)
            7200,   // Level 11->12 (+1320)
            8740,   // Level 12->13 (+1540)
            10000,  // Level 13->14 (+1260)
            10700,  // Level 14->15 (+700)
            11300,  // Level 15->16 (+600)
            12800,  // Level 16->17 (+1500)
            14400,  // Level 17->18 (+1600)
            16100,  // Level 18->19 (+1700)
            18000,  // Level 19->20 (+1900)
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
        Debug.Log(
            $"[StoryLevelData] Level={currentLevel}, XP={storyExperience}, Required={GetExperienceRequiredForNextLevel()}");

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
