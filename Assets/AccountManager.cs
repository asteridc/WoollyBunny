using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AccountManager : MonoBehaviour
{
    public static AccountManager Instance { get; private set; }

    private const string ACCOUNT_DATA_FILE = "account_data.json";
    private AccountData accountData;

    public event Action<int> OnStoryLevelChanged;
    public event Action<int> OnStoryExperienceChanged;
    public event Action OnStoryLevelUp;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAccountData();
        UpdateStoryLevelManager();

        Debug.Log($"[AccountManager] Инициализирован. УС: {accountData.storyLevelData.currentLevel}, Опыт: {accountData.storyLevelData.storyExperience}");
    }

    private void Start()
    {
        // Убеждаемся, что AccountManager существует
        if (Instance == null)
        {
            Debug.LogError("[AccountManager] Instance все еще null! Это не должно происходить.");
        }
    }

    public static AccountManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        Debug.LogWarning("[AccountManager] Instance был null, создаем новый GameObject...");
        GameObject go = new GameObject("AccountManager");
        return go.AddComponent<AccountManager>();
    }

    public void LoadAccountData()
    {
        string path = Path.Combine(
            Application.persistentDataPath,
            ACCOUNT_DATA_FILE);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            accountData = JsonUtility.FromJson<AccountData>(json);

            if (accountData == null)
                accountData = new AccountData();
        }
        else
        {
            accountData = new AccountData();
        }

        if (accountData.storyLevelData == null)
            accountData.storyLevelData = new StoryLevelData();

        if (accountData == null)
            accountData = new AccountData();

        if (accountData.storyLevelData == null)
            accountData.storyLevelData = new StoryLevelData();

        if (accountData.rewardedChapterNumbers == null)
            accountData.rewardedChapterNumbers = new System.Collections.Generic.List<int>();

        SaveAccountData();
    }

    public void SaveAccountData()
    {
        string path = Path.Combine(Application.persistentDataPath, ACCOUNT_DATA_FILE);
        string json = JsonUtility.ToJson(accountData, true);
        File.WriteAllText(path, json);
        Debug.Log($"[AccountManager] Данные аккаунта сохранены: {path}");
    }

    public void AddStoryExperience(int amount)
    {
        int previousLevel = accountData.storyLevelData.currentLevel;
        accountData.storyLevelData.AddExperience(amount);

        OnStoryExperienceChanged?.Invoke(accountData.storyLevelData.storyExperience);

        if (accountData.storyLevelData.currentLevel > previousLevel)
        {
            OnStoryLevelChanged?.Invoke(accountData.storyLevelData.currentLevel);
            OnStoryLevelUp?.Invoke();
            Debug.Log($"[AccountManager] УС повышен до {accountData.storyLevelData.currentLevel}!");
        }

        UpdateStoryLevelManager();
        SaveAccountData();
    }

    public void AddChapterExperience(int chapterNumber)
    {
        TryGrantChapterExperience(chapterNumber);
    }

    public bool TryGrantChapterExperience(int chapterNumber)
    {
        if (chapterNumber < 1)
        {
            Debug.LogWarning(
                $"[AccountManager] Некорректный номер главы: {chapterNumber}");

            return false;
        }

        if (accountData.rewardedChapterNumbers.Contains(chapterNumber))
        {
            Debug.Log(
                $"[AccountManager] Опыт за главу {chapterNumber} уже был выдан.");

            return false;
        }

        int reward = GetExperienceRewardForChapter(chapterNumber);

        // Сначала фиксируем награду, затем начисляем опыт.
        accountData.rewardedChapterNumbers.Add(chapterNumber);

        int previousLevel = accountData.storyLevelData.currentLevel;

        accountData.storyLevelData.AddExperience(reward);

        UpdateStoryLevelManager();
        SaveAccountData();

        OnStoryExperienceChanged?.Invoke(
            accountData.storyLevelData.storyExperience);

        if (accountData.storyLevelData.currentLevel > previousLevel)
        {
            OnStoryLevelChanged?.Invoke(
                accountData.storyLevelData.currentLevel);

            OnStoryLevelUp?.Invoke();
        }

        Debug.Log(
            $"[AccountManager] Выдано {reward} XP за главу {chapterNumber}.");

        return true;
    }

    public int SynchronizeCompletedChapterRewards(IEnumerable<int> completedChapters)
    {
        if (completedChapters == null)
        {
            Debug.LogWarning(
                "[AccountManager] Список пройденных глав отсутствует.");

            return 0;
        }

        int totalGrantedExperience = 0;

        foreach (int chapterNumber in completedChapters)
        {
            if (chapterNumber <= 0)
                continue;

            if (accountData.rewardedChapterNumbers.Contains(chapterNumber))
            {
                continue;
            }

            int reward = GetExperienceRewardForChapter(chapterNumber);

            accountData.rewardedChapterNumbers.Add(chapterNumber);

            accountData.storyLevelData.AddExperience(reward);
            totalGrantedExperience += reward;

            Debug.Log(
                $"[AccountManager] Восстановлена награда за главу " +
                $"{chapterNumber}: +{reward} XP.");
        }

        if (totalGrantedExperience <= 0)
            return 0;

        UpdateStoryLevelManager();
        SaveAccountData();

        OnStoryExperienceChanged?.Invoke(
            accountData.storyLevelData.storyExperience);

        OnStoryLevelChanged?.Invoke(
            accountData.storyLevelData.currentLevel);

        Debug.Log(
            $"[AccountManager] Синхронизация завершена. " +
            $"Начислено: {totalGrantedExperience} XP.");

        return totalGrantedExperience;
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

    public int GetCurrentLevel()
    {
        return accountData.storyLevelData.currentLevel;
    }

    public int GetCurrentExperience()
    {
        return accountData.storyLevelData.storyExperience;
    }

    public int GetTotalExperience()
    {
        return accountData.storyLevelData.totalStoryExperience;
    }

    public int GetExperienceRequiredForNextLevel()
    {
        return accountData.storyLevelData.GetExperienceRequiredForNextLevel();
    }

    public int GetExperienceUntilNextLevel()
    {
        return accountData.storyLevelData.GetExperienceUntilNextLevel();
    }

    public float GetLevelProgressPercent()
    {
        return accountData.storyLevelData.GetLevelProgressPercent();
    }

    public float GetHealthMultiplier()
    {
        int level = Mathf.Clamp(GetCurrentLevel(), 1, 20);
        return Mathf.Pow(1.30f, level - 1); // +30%
    }

    public float GetEnergyMultiplier()
    {
        int level = Mathf.Clamp(GetCurrentLevel(), 1, 20);
        return Mathf.Pow(1.05f, level - 1); // +5%
    }

    public float GetWeaponDamageMultiplier()
    {
        int level = Mathf.Clamp(GetCurrentLevel(), 1, 20);
        float bonus = 0f;

        for (int unlockedLevel = 2;
             unlockedLevel <= level;
             unlockedLevel++)
        {
            if (unlockedLevel <= 5)
                bonus += 0.20f;
            else if (unlockedLevel <= 10)
                bonus += 0.15f;
            else if (unlockedLevel <= 15)
                bonus += 0.10f;
            else
                bonus += 0.05f;
        }

        return 1f + bonus;
    }

    public int GetScaledHealth(int baseHealth)
    {
        return Mathf.Max(
            1,
            Mathf.RoundToInt(baseHealth * GetHealthMultiplier()));
    }

    public int GetScaledEnergy(int baseEnergy)
    {
        return Mathf.Max(
            1,
            Mathf.RoundToInt(baseEnergy * GetEnergyMultiplier()));
    }

    public int GetScaledWeaponDamage(int baseDamage)
    {
        return Mathf.Max(
            1,
            Mathf.RoundToInt(baseDamage * GetWeaponDamageMultiplier()));
    }

    public StoryLevelData GetStoryLevelData()
    {
        return accountData.storyLevelData;
    }

    public void SetStoryLevelData(StoryLevelData data)
    {
        accountData.storyLevelData = data;
        UpdateStoryLevelManager();
        SaveAccountData();
    }

    private void UpdateStoryLevelManager()
    {
        if (StoryLevelManager.Instance != null)
        {
            StoryLevelManager.Instance.SetData(accountData.storyLevelData);
        }
    }

    private void OnApplicationQuit()
    {
        SaveAccountData();
    }
}
