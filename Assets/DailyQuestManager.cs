using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class DailyQuestManager : MonoBehaviour
{
    [Serializable]
    private class QuestProgress
    {
        public string questId;
        public int progress;
    }

    [Serializable]
    private class PendingAction
    {
        public string actionId;
        public int amount;
    }

    [Serializable]
    private class DailyQuestSaveData
    {
        public string utcDate;
        public List<QuestProgress> progress = new List<QuestProgress>();
        public List<string> claimedRewards = new List<string>();
        public List<string> registeredOnceActions = new List<string>();
        public List<PendingAction> pendingChapterActions = new List<PendingAction>();
    }

    public static DailyQuestManager Instance { get; private set; }

    [Header("Quest Pool")]
    [SerializeField] private DailyQuestDefinition[] questPool;
    [SerializeField, Min(1)] private int dailyQuestCount = 3;

    [Header("UI Slots")]
    [SerializeField] private DailyQuestSlot[] questSlots;
    [SerializeField] private CountdownTimer resetCountdown;
    [SerializeField] private string resetCountdownId = "DailyQuestReset";
    [SerializeField] private DailyQuestRewardNotificationUI rewardNotificationUI;

    private const string SaveFileName = "daily_quests.json";
    private DailyQuestSaveData saveData;
    private readonly List<DailyQuestDefinition> activeQuests = new List<DailyQuestDefinition>();

    public event Func<string, int, bool> CustomRewardRequested;
    public event Action QuestsUpdated;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadOrCreateDailyQuests();
    }

    public static DailyQuestManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        GameObject managerObject = new GameObject(nameof(DailyQuestManager));
        return managerObject.AddComponent<DailyQuestManager>();
    }

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (saveData != null)
        {
            BindSlots();
            RefreshUI();
            UpdateResetTimer();
        }
    }

    public void RegisterActionOncePerQuest(string actionId, string questId, int amount = 1)
    {
        EnsureCurrentUtcDay();

        if (string.IsNullOrWhiteSpace(actionId) || string.IsNullOrWhiteSpace(questId) || amount <= 0)
            return;

        DailyQuestDefinition quest = activeQuests.Find(item => item.id == questId);
        if (quest == null || quest.actionId != actionId || IsCompleted(quest))
            return;

        string onceKey = actionId + "|" + questId;
        if (saveData.registeredOnceActions.Contains(onceKey))
            return;

        saveData.registeredOnceActions.Add(onceKey);
        QuestProgress progress = GetProgress(quest.id);
        progress.progress = Mathf.Min(quest.requiredCount, progress.progress + amount);
        Save();
        RefreshUI();
        QuestsUpdated?.Invoke();
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    private void OnApplicationQuit()
    {
        // При выходе приложения сохраняем накопленные очки,
        // но не засчитываем выход из главы, пока глава фактически не завершена.
        Save();
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        DailyQuestSlot[] sceneSlots = FindObjectsOfType<DailyQuestSlot>(true);
        questSlots = sceneSlots;

        CountdownTimer[] timers = FindObjectsOfType<CountdownTimer>(true);
        resetCountdown = null;
        foreach (CountdownTimer timer in timers)
        {
            if (timer.TimerId == resetCountdownId)
            {
                resetCountdown = timer;
                break;
            }
        }

        BindSlots();
        RefreshUI();
        UpdateResetTimer();
    }

    private void Update()
    {
        EnsureCurrentUtcDay();
    }

    public void RegisterAction(string actionId, int amount = 1)
    {
        EnsureCurrentUtcDay();

        if (string.IsNullOrWhiteSpace(actionId) || amount <= 0)
            return;

        ApplyActionProgress(actionId, amount);
    }

    public void RegisterReachedDialogueLine(string chapterId, int chapterNumber, int lineIndex)
    {
        EnsureCurrentUtcDay();

        for (int i = 0; i < activeQuests.Count; i++)
        {
            DailyQuestDefinition quest = activeQuests[i];
            if (quest.dialogueLineIndex != lineIndex + 1 ||
                (!string.IsNullOrEmpty(quest.chapterId) && quest.chapterId != chapterId) ||
                (quest.chapterNumber > 0 && quest.chapterNumber != chapterNumber))
                continue;

            ApplyActionProgress(quest.actionId, 1);
        }
    }

    public void AccumulateActionUntilChapterExit(string actionId, int amount = 1)
    {
        EnsureCurrentUtcDay();

        if (string.IsNullOrWhiteSpace(actionId) || amount <= 0)
            return;

        PendingAction pending = saveData.pendingChapterActions.Find(
            item => item.actionId == actionId);

        if (pending == null)
        {
            pending = new PendingAction { actionId = actionId, amount = 0 };
            saveData.pendingChapterActions.Add(pending);
        }

        pending.amount += amount;

        if (!activeQuests.Exists(quest => quest.actionId == actionId))
        {
            saveData.pendingChapterActions.Remove(pending);
        }

        Save();
    }

    public void CommitPendingChapterActions()
    {
        EnsureCurrentUtcDay();

        if (saveData.pendingChapterActions.Count == 0)
            return;

        foreach (PendingAction pending in saveData.pendingChapterActions)
            ApplyActionProgress(pending.actionId, pending.amount, false);

        saveData.pendingChapterActions.Clear();
        Save();
        RefreshUI();
        QuestsUpdated?.Invoke();
    }

    public void RegisterActionOncePerDay(string actionId, int amount = 1)
    {
        EnsureCurrentUtcDay();

        if (string.IsNullOrWhiteSpace(actionId) || amount <= 0 ||
            saveData.registeredOnceActions.Contains(actionId))
            return;

        bool changed = false;
        foreach (DailyQuestDefinition quest in activeQuests)
        {
            if (quest.actionId != actionId || IsCompleted(quest))
                continue;

            QuestProgress progress = GetProgress(quest.id);
            progress.progress = Mathf.Min(quest.requiredCount, progress.progress + amount);
            changed = true;
        }

        if (changed)
        {
            saveData.registeredOnceActions.Add(actionId);
            Save();
            RefreshUI();
            QuestsUpdated?.Invoke();
        }
    }

    public bool ClaimReward(int questIndex)
    {
        EnsureCurrentUtcDay();

        if (questIndex < 0 || questIndex >= activeQuests.Count)
            return false;

        DailyQuestDefinition quest = activeQuests[questIndex];
        if (!IsCompleted(quest))
            return false;

        string claimKey = GetClaimKey(quest.id);
        if (saveData.claimedRewards.Contains(claimKey))
            return false;

        bool rewardGranted;

        switch (quest.rewardType)
        {
            case DailyQuestRewardType.StoryExperience:
                rewardGranted = AccountManager.Instance != null;
                if (rewardGranted)
                    AccountManager.Instance.AddStoryExperience(quest.rewardAmount);
                break;
            case DailyQuestRewardType.BloodthirstPoints:
                rewardGranted = GrantPathPoints(HeroPath.Bloodthirst, quest.rewardAmount);
                break;
            case DailyQuestRewardType.NoblePoints:
                rewardGranted = GrantPathPoints(HeroPath.Noble, quest.rewardAmount);
                break;
            case DailyQuestRewardType.LovePoints:
                rewardGranted = GrantPathPoints(HeroPath.Love, quest.rewardAmount);
                break;
            case DailyQuestRewardType.InventoryEntry:
                rewardGranted = ResourceInventoryManager.Instance != null &&
                    ResourceInventoryManager.Instance.GrantReward(
                        quest.inventoryEntryId,
                        quest.rewardAmount);
                break;
            case DailyQuestRewardType.Custom:
                rewardGranted = GrantCustomReward(
                    quest.customRewardId,
                    quest.rewardAmount);
                break;
            default:
                rewardGranted = false;
                break;
        }

        if (!rewardGranted)
        {
            Debug.LogWarning($"DailyQuestManager: не удалось выдать награду за квест '{quest.id}'.", this);
            return false;
        }

        saveData.claimedRewards.Add(claimKey);
        Save();
        RefreshUI();
        if (rewardNotificationUI == null)
            rewardNotificationUI = FindObjectOfType<DailyQuestRewardNotificationUI>(true);
        rewardNotificationUI?.ShowReward(quest);
        QuestsUpdated?.Invoke();
        return true;
    }

    public bool IsRewardClaimed(int questIndex)
    {
        return questIndex >= 0 && questIndex < activeQuests.Count &&
            saveData.claimedRewards.Contains(GetClaimKey(activeQuests[questIndex].id));
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [ContextMenu("Developer/Refresh Daily Quests")]
    private void RefreshQuestsForDevelopment()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("DailyQuestManager: обновлять квесты можно только во время игры.", this);
            return;
        }

        EnsureCurrentUtcDay();
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        string claimPrefix = today + "|";
        saveData.claimedRewards.RemoveAll(key => key.StartsWith(claimPrefix, StringComparison.Ordinal));

        HashSet<string> previousQuestIds = new HashSet<string>();
        foreach (DailyQuestDefinition quest in activeQuests)
            previousQuestIds.Add(quest.id);

        CreateDailyQuests(today, previousQuestIds);
        RefreshUI();
        UpdateResetTimer();
        QuestsUpdated?.Invoke();
    }
#endif

    private bool GrantPathPoints(HeroPath path, int amount)
    {
        if (HeroStats.Instance != null)
        {
            HeroStats.Instance.AddPoints(path, amount);
            return true;
        }

        return false;
    }

    private bool GrantCustomReward(string rewardId, int amount)
    {
        if (CustomRewardRequested == null)
            return false;

        foreach (Delegate callback in CustomRewardRequested.GetInvocationList())
        {
            try
            {
                if (((Func<string, int, bool>)callback).Invoke(rewardId, amount))
                    return true;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, this);
            }
        }

        return false;
    }

    private void LoadOrCreateDailyQuests()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        try
        {
            string serializedData = AccountManager.GetOrCreate().GetDailyQuestProgressJson();
            if (string.IsNullOrWhiteSpace(serializedData) && File.Exists(SavePath))
                serializedData = File.ReadAllText(SavePath);

            if (!string.IsNullOrWhiteSpace(serializedData))
                saveData = JsonUtility.FromJson<DailyQuestSaveData>(serializedData);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"DailyQuestManager: файл прогресса повреждён: {exception.Message}", this);
        }

        if (saveData == null)
            saveData = new DailyQuestSaveData();

        if (saveData.progress == null)
            saveData.progress = new List<QuestProgress>();
        if (saveData.claimedRewards == null)
            saveData.claimedRewards = new List<string>();
        if (saveData.registeredOnceActions == null)
            saveData.registeredOnceActions = new List<string>();
        if (saveData.pendingChapterActions == null)
            saveData.pendingChapterActions = new List<PendingAction>();

        if (saveData.utcDate != today || saveData.progress.Count == 0)
            CreateDailyQuests(today);
        else
            RestoreActiveQuests();

        if (saveData.pendingChapterActions.Count > 0)
        {
            foreach (PendingAction pending in saveData.pendingChapterActions)
                ApplyActionProgress(pending.actionId, pending.amount, false);

            saveData.pendingChapterActions.Clear();
            Save();
        }

        if (saveData.utcDate == today && saveData.progress.Count == 0 && activeQuests.Count == 0)
            Save();
    }

    private void CreateDailyQuests(string utcDate, HashSet<string> previouslyActiveQuestIds = null)
    {
        saveData.utcDate = utcDate;
        saveData.progress.Clear();
        saveData.registeredOnceActions.Clear();
        saveData.pendingChapterActions.Clear();
        activeQuests.Clear();

        if (questPool == null || questPool.Length == 0)
        {
            Save();
            Debug.LogWarning("DailyQuestManager: список квестов пуст.", this);
            return;
        }

        List<DailyQuestDefinition> candidates = new List<DailyQuestDefinition>();
        HashSet<string> seenIds = new HashSet<string>();
        foreach (DailyQuestDefinition quest in questPool)
        {
            if (quest != null && !string.IsNullOrWhiteSpace(quest.id) &&
                quest.requiredCount > 0 && seenIds.Add(quest.id))
                candidates.Add(quest);
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            int swapIndex = UnityEngine.Random.Range(i, candidates.Count);
            DailyQuestDefinition temp = candidates[i];
            candidates[i] = candidates[swapIndex];
            candidates[swapIndex] = temp;
        }

        if (previouslyActiveQuestIds != null)
        {
            List<DailyQuestDefinition> unusedCandidates = candidates.FindAll(
                quest => !previouslyActiveQuestIds.Contains(quest.id));
            List<DailyQuestDefinition> previousCandidates = candidates.FindAll(
                quest => previouslyActiveQuestIds.Contains(quest.id));
            unusedCandidates.AddRange(previousCandidates);
            candidates = unusedCandidates;
        }

        int availableSlotCount = questSlots != null && questSlots.Length > 0
            ? questSlots.Length
            : dailyQuestCount;
        int count = Mathf.Min(dailyQuestCount, candidates.Count, availableSlotCount);
        for (int i = 0; i < count; i++)
        {
            DailyQuestDefinition quest = candidates[i];
            activeQuests.Add(quest);
            saveData.progress.Add(new QuestProgress { questId = quest.id, progress = 0 });
        }

        Save();
    }

    private void RestoreActiveQuests()
    {
        activeQuests.Clear();
        foreach (QuestProgress saved in saveData.progress)
        {
            DailyQuestDefinition definition = FindDefinition(saved.questId);
            if (definition != null)
                activeQuests.Add(definition);
        }
    }

    private void BindSlots()
    {
        if (questSlots == null)
            return;

        for (int i = 0; i < questSlots.Length; i++)
        {
            if (questSlots[i] != null)
                questSlots[i].Bind(this, i);
        }
    }

    private void RefreshUI()
    {
        if (questSlots == null)
            return;

        for (int i = 0; i < questSlots.Length; i++)
        {
            if (questSlots[i] == null)
                continue;

            DailyQuestDefinition quest = i < activeQuests.Count ? activeQuests[i] : null;
            if (quest == null)
            {
            questSlots[i].Refresh(null, 0, false, false, LanguageManager.CurrentLanguage);
                continue;
            }

            QuestProgress progress = GetProgress(quest.id);
        questSlots[i].Refresh(
            quest,
            progress.progress,
            IsCompleted(quest),
            saveData.claimedRewards.Contains(GetClaimKey(quest.id)),
            LanguageManager.CurrentLanguage);
        }
    }

    private QuestProgress GetProgress(string questId)
    {
        QuestProgress progress = saveData.progress.Find(item => item.questId == questId);
        if (progress == null)
        {
            progress = new QuestProgress { questId = questId, progress = 0 };
            saveData.progress.Add(progress);
        }

        return progress;
    }

    private bool IsCompleted(DailyQuestDefinition quest)
    {
        QuestProgress progress = GetProgress(quest.id);
        return progress.progress >= quest.requiredCount;
    }

    private DailyQuestDefinition FindDefinition(string id)
    {
        if (questPool == null)
            return null;

        foreach (DailyQuestDefinition quest in questPool)
        {
            if (quest != null && quest.id == id)
                return quest;
        }

        return null;
    }

    private string GetClaimKey(string questId)
    {
        return saveData.utcDate + "|" + questId;
    }

    private void UpdateResetTimer()
    {
        if (resetCountdown == null)
            return;

        DateTime nextResetUtc = DateTime.UtcNow.Date.AddDays(1);
        resetCountdown.SetTargetUtc(nextResetUtc);
    }

    private void EnsureCurrentUtcDay()
    {
        string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        if (saveData == null)
        {
            LoadOrCreateDailyQuests();
            BindSlots();
            RefreshUI();
            UpdateResetTimer();
            return;
        }

        if (saveData.utcDate == today)
            return;

        CreateDailyQuests(today);
        RefreshUI();
        UpdateResetTimer();
        QuestsUpdated?.Invoke();
    }

    private bool ApplyActionProgress(string actionId, int amount, bool saveImmediately = true)
    {
        bool changed = false;
        for (int i = 0; i < activeQuests.Count; i++)
        {
            DailyQuestDefinition quest = activeQuests[i];
            if (quest.actionId != actionId || IsCompleted(quest))
                continue;

            QuestProgress progress = GetProgress(quest.id);
            progress.progress = Mathf.Min(quest.requiredCount, progress.progress + amount);
            changed = true;
        }

        if (!changed)
            return false;

        if (saveImmediately)
        {
            Save();
            RefreshUI();
            QuestsUpdated?.Invoke();
        }

        return true;
    }

    private void Save()
    {
        try
        {
            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            AccountManager.GetOrCreate().SaveDailyQuestProgress(json);
        }
        catch (Exception exception)
        {
            Debug.LogError($"DailyQuestManager: не удалось сохранить прогресс: {exception.Message}", this);
        }
    }
}
