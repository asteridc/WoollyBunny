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
        if (accountData.inventoryAmounts == null)
            accountData.inventoryAmounts = new List<AccountInventoryAmount>();
        if (accountData.ownedUniqueInventoryItems == null)
            accountData.ownedUniqueInventoryItems = new List<string>();

        NormalizeInventoryData();
        SaveAccountData();
    }

    public int GetInventoryAmount(string id)
    {
        string normalizedId = NormalizeInventoryId(id);
        if (normalizedId == null)
            return 0;

        int total = 0;
        foreach (AccountInventoryAmount entry in accountData.inventoryAmounts)
        {
            if (entry != null && InventoryIdsEqual(entry.id, normalizedId))
                total += Mathf.Max(0, entry.amount);
        }

        return total;
    }

    public bool HasInventoryAmountRecord(string id)
    {
        string normalizedId = NormalizeInventoryId(id);
        return normalizedId != null && accountData.inventoryAmounts.Exists(
            item => item != null && InventoryIdsEqual(item.id, normalizedId));
    }

    public bool SetInventoryAmount(string id, int amount)
    {
        string normalizedId = NormalizeInventoryId(id);
        if (normalizedId == null || amount < 0)
            return false;

        AccountInventoryAmount entry = GetOrCreateInventoryAmount(normalizedId);
        entry.amount = amount;
        SaveAccountData();
        return true;
    }

    public bool ExchangeInventoryAmounts(
        string fromId,
        int fromAmount,
        string toId,
        int toAmount)
    {
        string normalizedFromId = NormalizeInventoryId(fromId);
        string normalizedToId = NormalizeInventoryId(toId);
        if (normalizedFromId == null || normalizedToId == null ||
            InventoryIdsEqual(normalizedFromId, normalizedToId) ||
            fromAmount <= 0 || toAmount <= 0 ||
            GetInventoryAmount(normalizedFromId) < fromAmount)
            return false;

        AccountInventoryAmount from = GetOrCreateInventoryAmount(normalizedFromId);
        AccountInventoryAmount to = GetOrCreateInventoryAmount(normalizedToId);
        from.amount -= fromAmount;
        to.amount += toAmount;
        SaveAccountData();
        return true;
    }

    public bool ExchangeUniqueInventoryItems(string fromId, string toId)
    {
        string normalizedFromId = NormalizeInventoryId(fromId);
        string normalizedToId = NormalizeInventoryId(toId);
        if (normalizedFromId == null || normalizedToId == null ||
            InventoryIdsEqual(normalizedFromId, normalizedToId) ||
            !HasUniqueInventoryItem(normalizedFromId) ||
            HasUniqueInventoryItem(normalizedToId))
            return false;

        RemoveUniqueInventoryItem(normalizedFromId);
        accountData.ownedUniqueInventoryItems.Add(normalizedToId);
        SaveAccountData();
        return true;
    }

    public AccountData GetAccountDataSnapshot()
    {
        return JsonUtility.FromJson<AccountData>(JsonUtility.ToJson(accountData));
    }

    public void RestoreAccountDataSnapshot(AccountData snapshot)
    {
        if (snapshot == null)
            return;

        accountData = snapshot;
        if (accountData.inventoryAmounts == null)
            accountData.inventoryAmounts = new List<AccountInventoryAmount>();
        if (accountData.ownedUniqueInventoryItems == null)
            accountData.ownedUniqueInventoryItems = new List<string>();
        NormalizeInventoryData();
        SaveAccountData();
    }

    private AccountInventoryAmount GetOrCreateInventoryAmount(string id)
    {
        string normalizedId = NormalizeInventoryId(id);
        AccountInventoryAmount entry = null;
        for (int i = 0; i < accountData.inventoryAmounts.Count;)
        {
            AccountInventoryAmount candidate = accountData.inventoryAmounts[i];
            if (candidate == null || !InventoryIdsEqual(candidate.id, normalizedId))
            {
                i++;
                continue;
            }

            if (entry == null)
            {
                entry = candidate;
                entry.id = normalizedId;
                entry.amount = Mathf.Max(0, entry.amount);
                i++;
            }
            else
            {
                entry.amount += Mathf.Max(0, candidate.amount);
                accountData.inventoryAmounts.RemoveAt(i);
            }
        }

        if (entry != null)
            return entry;

        entry = new AccountInventoryAmount { id = normalizedId, amount = 0 };
        accountData.inventoryAmounts.Add(entry);
        return entry;
    }

    public bool HasUniqueInventoryItem(string id)
    {
        string normalizedId = NormalizeInventoryId(id);
        return normalizedId != null && accountData.ownedUniqueInventoryItems.Exists(
            item => InventoryIdsEqual(item, normalizedId));
    }

    public bool SetUniqueInventoryItemOwned(string id, bool owned)
    {
        string normalizedId = NormalizeInventoryId(id);
        if (normalizedId == null)
            return false;

        if (owned)
        {
            if (!HasUniqueInventoryItem(normalizedId))
                accountData.ownedUniqueInventoryItems.Add(normalizedId);
        }
        else
        {
            RemoveUniqueInventoryItem(normalizedId);
        }

        SaveAccountData();
        return true;
    }

    public void MarkInventoryInitialized()
    {
        if (accountData.inventoryInitialized)
            return;

        accountData.inventoryInitialized = true;
        SaveAccountData();
    }

    public bool IsInventoryInitialized => accountData.inventoryInitialized;

    private void NormalizeInventoryData()
    {
        Dictionary<string, AccountInventoryAmount> amountsById =
            new Dictionary<string, AccountInventoryAmount>(StringComparer.OrdinalIgnoreCase);
        List<AccountInventoryAmount> normalizedAmounts = new List<AccountInventoryAmount>();

        foreach (AccountInventoryAmount amount in accountData.inventoryAmounts)
        {
            if (amount == null)
                continue;

            string normalizedId = NormalizeInventoryId(amount.id);
            if (normalizedId == null)
                continue;

            if (amountsById.TryGetValue(normalizedId, out AccountInventoryAmount existing))
            {
                existing.amount += Mathf.Max(0, amount.amount);
                continue;
            }

            amount.id = normalizedId;
            amount.amount = Mathf.Max(0, amount.amount);
            amountsById.Add(normalizedId, amount);
            normalizedAmounts.Add(amount);
        }

        accountData.inventoryAmounts = normalizedAmounts;

        List<string> normalizedUniqueItems = new List<string>();
        foreach (string itemId in accountData.ownedUniqueInventoryItems)
        {
            string normalizedId = NormalizeInventoryId(itemId);
            if (normalizedId != null && !normalizedUniqueItems.Exists(
                existing => InventoryIdsEqual(existing, normalizedId)))
                normalizedUniqueItems.Add(normalizedId);
        }

        accountData.ownedUniqueInventoryItems = normalizedUniqueItems;
    }

    private void RemoveUniqueInventoryItem(string id)
    {
        accountData.ownedUniqueInventoryItems.RemoveAll(
            item => InventoryIdsEqual(item, id));
    }

    private static string NormalizeInventoryId(string id)
    {
        return string.IsNullOrWhiteSpace(id) ? null : id.Trim();
    }

    private static bool InventoryIdsEqual(string first, string second)
    {
        string normalizedFirst = NormalizeInventoryId(first);
        string normalizedSecond = NormalizeInventoryId(second);
        return normalizedFirst != null && normalizedSecond != null &&
            string.Equals(normalizedFirst, normalizedSecond, StringComparison.OrdinalIgnoreCase);
    }

    public string GetDailyQuestProgressJson()
    {
        return accountData.dailyQuestProgressJson;
    }

    public void SaveDailyQuestProgress(string json)
    {
        accountData.dailyQuestProgressJson = json;
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
