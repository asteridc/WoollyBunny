using UnityEngine;
using System;

public class StoryLevelManager : MonoBehaviour
{
    public static StoryLevelManager Instance { get; private set; }

    [SerializeField] private StoryLevelData storyLevelData;

    public event Action<int> OnLevelChanged;
    public event Action<int> OnExperienceChanged;
    public event Action OnLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (storyLevelData == null)
            storyLevelData = new StoryLevelData();
    }

    public int GetCurrentLevel()
    {
        return storyLevelData.currentLevel;
    }

    public int GetCurrentExperience()
    {
        return storyLevelData.storyExperience;
    }

    public int GetTotalExperience()
    {
        return storyLevelData.totalStoryExperience;
    }

    public int GetExperienceRequiredForNextLevel()
    {
        return storyLevelData.GetExperienceRequiredForNextLevel();
    }

    public int GetExperienceUntilNextLevel()
    {
        return storyLevelData.GetExperienceUntilNextLevel();
    }

    public float GetLevelProgressPercent()
    {
        return storyLevelData.GetLevelProgressPercent();
    }

    public void AddStoryExperience(int amount)
    {
        int previousLevel = storyLevelData.currentLevel;
        storyLevelData.AddExperience(amount);

        OnExperienceChanged?.Invoke(storyLevelData.storyExperience);

        if (storyLevelData.currentLevel > previousLevel)
        {
            OnLevelChanged?.Invoke(storyLevelData.currentLevel);
            OnLevelUp?.Invoke();

            Debug.Log($"[StoryLevel] Уровень повышен до {storyLevelData.currentLevel}!");
        }
    }

    public void AddChapterExperience(int chapterNumber)
    {
        int experienceReward = GetExperienceRewardForChapter(chapterNumber);
        AddStoryExperience(experienceReward);
    }

    private int GetExperienceRewardForChapter(int chapterNumber)
    {
        return chapterNumber switch
        {
            1 => 500,
            2 => 850,
            3 => 1200,
            4 => 1600,
            5 => 2050,
            _ => 500
        };
    }

    public float GetHealthMultiplier()
    {
        // На каждом уровне здоровье повышается на 30%
        // Уровень 1: 1.0x
        // Уровень 2: 1.3x
        // Уровень 3: 1.69x
        float baseMultiplier = 1.0f;
        for (int i = 1; i < storyLevelData.currentLevel; i++)
        {
            baseMultiplier *= 1.3f;
        }
        return baseMultiplier;
    }

    public float GetEnergyMultiplier()
    {
        // На каждом уровне энергия повышается на 5%
        // Уровень 1: 1.0x
        // Уровень 2: 1.05x
        // Уровень 3: 1.1025x
        float baseMultiplier = 1.0f;
        for (int i = 1; i < storyLevelData.currentLevel; i++)
        {
            baseMultiplier *= 1.05f;
        }
        return baseMultiplier;
    }

    public float GetWeaponDamageMultiplier()
    {
        // На каждом уровне урон оружия повышается на 40%
        // Уровень 1: 1.0x
        // Уровень 2: 1.4x
        // Уровень 3: 1.96x
        float baseMultiplier = 1.0f;
        for (int i = 1; i < storyLevelData.currentLevel; i++)
        {
            baseMultiplier *= 1.4f;
        }
        return baseMultiplier;
    }

    public void SetData(StoryLevelData data)
    {
        storyLevelData = data;
    }

    public StoryLevelData GetData()
    {
        return storyLevelData;
    }
}
