using UnityEngine;
using System;

public class StoryLevelManager : MonoBehaviour
{
    public static StoryLevelManager Instance { get; private set; }

    private const int MaxStoryLevel = 20;

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
        if (amount <= 0)
            return;

        int previousLevel = storyLevelData.currentLevel;
        storyLevelData.AddExperience(amount);

        OnExperienceChanged?.Invoke(storyLevelData.storyExperience);

        if (storyLevelData.currentLevel > previousLevel)
        {
            OnLevelChanged?.Invoke(storyLevelData.currentLevel);
            OnLevelUp?.Invoke();

            Debug.Log(
                $"[StoryLevel] Уровень повышен до {storyLevelData.currentLevel}!");
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
        // Линейный рост: +15% максимального здоровья за уровень.
        return 1f + (GetSafeLevel() - 1) * 0.15f;
    }

    public float GetEnergyMultiplier()
    {
        // Линейный рост: +5% максимальной энергии за уровень.
        return 1f + (GetSafeLevel() - 1) * 0.05f;
    }

    public float GetWeaponDamageMultiplier()
    {
        int level = GetSafeLevel();
        float bonus = 0f;

        for (int unlockedLevel = 2;
             unlockedLevel <= level;
             unlockedLevel++)
        {
            bonus += GetWeaponDamageBonusForLevel(unlockedLevel);
        }

        return 1f + bonus;
    }

    public int GetScaledHealth(int baseHealth)
    {
        return ScaleWholeNumber(baseHealth, GetHealthMultiplier());
    }

    public int GetScaledEnergy(int baseEnergy)
    {
        return ScaleWholeNumber(baseEnergy, GetEnergyMultiplier());
    }

    public int GetScaledWeaponDamage(int baseDamage)
    {
        return ScaleWholeNumber(baseDamage, GetWeaponDamageMultiplier());
    }

    public float GetScaledWeaponDamage(float baseDamage)
    {
        return Mathf.Max(0f, baseDamage * GetWeaponDamageMultiplier());
    }

    public void SetData(StoryLevelData data)
    {
        storyLevelData = data ?? new StoryLevelData();
    }

    public StoryLevelData GetData()
    {
        return storyLevelData;
    }

    private int GetSafeLevel()
    {
        if (storyLevelData == null)
            return 1;

        return Mathf.Clamp(storyLevelData.currentLevel, 1, MaxStoryLevel);
    }

    private static float GetWeaponDamageBonusForLevel(int level)
    {
        if (level <= 5)
            return 0.20f;

        if (level <= 10)
            return 0.15f;

        if (level <= 15)
            return 0.10f;

        return 0.05f;
    }

    private static int ScaleWholeNumber(int baseValue, float multiplier)
    {
        if (baseValue <= 0)
            return 0;

        return Mathf.Max(1, Mathf.RoundToInt(baseValue * multiplier));
    }
}