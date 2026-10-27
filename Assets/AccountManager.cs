using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-200)]
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
            Destroy(this);
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

        if (accountData.halloweenPromoCodes == null)
            accountData.halloweenPromoCodes =
                new List<HalloweenPromoCodeRecord>();

        accountData.halloweenBasketCapacityUsed =
            Mathf.Clamp(accountData.halloweenBasketCapacityUsed,0,100);

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

    
    // ----- Halloween Exchange: account-level persistence -----

    public int GetHalloweenBasketCapacityUsed()
    {
        return accountData == null
            ? 0
            : Mathf.Clamp(
                accountData.halloweenBasketCapacityUsed,
                0,
                100);
    }

    public List<HalloweenPromoCodeRecord>
        GetHalloweenPromoCodeSnapshot()
    {
        List<HalloweenPromoCodeRecord> snapshot =
            new List<HalloweenPromoCodeRecord>();

        if (accountData == null ||
            accountData.halloweenPromoCodes == null)
            return snapshot;

        foreach (HalloweenPromoCodeRecord record
                 in accountData.halloweenPromoCodes)
        {
            if (record != null)
                snapshot.Add(record.Copy());
        }

        return snapshot;
    }

    public HalloweenPromoCodeRecord FindHalloweenPromoCode(
        string code)
    {
        if (accountData == null ||
            accountData.halloweenPromoCodes == null ||
            string.IsNullOrWhiteSpace(code))
            return null;

        HalloweenPromoCodeRecord record =
            accountData.halloweenPromoCodes.Find(item =>
                item != null &&
                string.Equals(
                    item.code,
                    code.Trim(),
                    StringComparison.OrdinalIgnoreCase));

        return record != null ? record.Copy() : null;
    }

    public bool HasHalloweenPromoCode(string code)
    {
        return FindHalloweenPromoCode(code) != null;
    }

    public bool HasHalloweenPromoForReward(
        string rewardInventoryId)
    {
        if (accountData == null ||
            accountData.halloweenPromoCodes == null ||
            string.IsNullOrWhiteSpace(rewardInventoryId))
            return false;

        return accountData.halloweenPromoCodes.Exists(record =>
            record != null &&
            InventoryIdsEqual(
                record.rewardInventoryId,
                rewardInventoryId));
    }

    public long GetPumpkinLanternLastAccrualUnixSeconds()
    {
        return accountData != null
            ? accountData.pumpkinLanternLastAccrualUnixSeconds
            : 0;
    }

    public bool InitializePumpkinLanternIfNeeded(long initialUnixTimestamp)
    {
        if (accountData == null || initialUnixTimestamp <= 0)
            return false;

        if (accountData.pumpkinLanternLastAccrualUnixSeconds > 0)
            return true;

        accountData.pumpkinLanternLastAccrualUnixSeconds = initialUnixTimestamp;
        try
        {
            SaveAccountData();
            return true;
        }
        catch (Exception exception)
        {
            accountData.pumpkinLanternLastAccrualUnixSeconds = 0;
            Debug.LogError($"[AccountManager] Не удалось инициализировать фонарь: {exception.Message}", this);
            return false;
        }
    }

    // Баланс и время следующего накопления сохраняются одной записью аккаунта.
    // Это не позволяет выдать конфеты повторно, если игра завершится между двумя отдельными сохранениями.
    public bool TryCollectPumpkinLantern(
        string resourceId,
        int amount,
        long nextAccrualUnixTimestamp,
        out int newBalance)
    {
        newBalance = 0;
        string normalizedId = NormalizeInventoryId(resourceId);

        if (accountData == null || normalizedId == null || amount <= 0 ||
            nextAccrualUnixTimestamp <= 0)
            return false;

        AccountInventoryAmount entry = GetOrCreateInventoryAmount(normalizedId);
        int previousAmount = Mathf.Max(0, entry.amount);
        long previousTimestamp = accountData.pumpkinLanternLastAccrualUnixSeconds;

        if (previousAmount > int.MaxValue - amount)
            return false;

        entry.amount = previousAmount + amount;
        accountData.pumpkinLanternLastAccrualUnixSeconds = nextAccrualUnixTimestamp;

        try
        {
            SaveAccountData();
            newBalance = entry.amount;
            return true;
        }
        catch (Exception exception)
        {
            entry.amount = previousAmount;
            accountData.pumpkinLanternLastAccrualUnixSeconds = previousTimestamp;
            Debug.LogError($"[AccountManager] Не удалось сохранить сбор фонаря: {exception.Message}", this);
            return false;
        }
    }

    public bool TryPurchaseHalloweenOffer(
    string currencyId,
    string offerId,
    string rewardInventoryId,
    int price,
    int capacityPercent,
    string code,
    long createdAtUnixSeconds,
    long expiresAtUnixSeconds,
    out string failureReason)
    {
        failureReason = string.Empty;

        string normalizedCurrencyId =
            NormalizeInventoryId(currencyId);
        string normalizedOfferId =
            NormalizeInventoryId(offerId);
        string normalizedRewardId =
            NormalizeInventoryId(rewardInventoryId);

        string normalizedCode = string.IsNullOrWhiteSpace(code)
            ? string.Empty
            : code.Trim().ToUpperInvariant();

        if (accountData == null ||
            normalizedCurrencyId == null ||
            normalizedOfferId == null ||
            normalizedRewardId == null ||
            price <= 0 ||
            capacityPercent < 1 ||
            capacityPercent > 100)
        {
            return FailHalloweenExchange(
                "Некорректные параметры покупки.",
                out failureReason);
        }

        if (!HalloweenPromoCodeService.IsValidFormat(normalizedCode) ||
            HasHalloweenPromoCode(normalizedCode))
        {
            return FailHalloweenExchange(
                "Промокод имеет неверный формат или уже существует.",
                out failureReason);
        }

        if (createdAtUnixSeconds <= 0 ||
            expiresAtUnixSeconds <= createdAtUnixSeconds)
        {
            return FailHalloweenExchange(
                "Некорректный срок действия промокода.",
                out failureReason);
        }

        if (GetHalloweenBasketCapacityUsed() + capacityPercent > 100)
        {
            return FailHalloweenExchange(
                "В корзине недостаточно места.",
                out failureReason);
        }

        if (HasUniqueInventoryItem(normalizedRewardId) ||
            HasHalloweenPromoForReward(normalizedRewardId))
        {
            return FailHalloweenExchange(
                "Эта уникальная награда уже получена или куплена.",
                out failureReason);
        }

        if (GetInventoryAmount(normalizedCurrencyId) < price)
        {
            return FailHalloweenExchange(
                "Недостаточно Конфетыкв.",
                out failureReason);
        }

        AccountInventoryAmount currencyEntry =
            GetOrCreateInventoryAmount(normalizedCurrencyId);

        int previousBalance = currencyEntry.amount;
        int previousCapacity = accountData.halloweenBasketCapacityUsed;

        HalloweenPromoCodeRecord record =
            new HalloweenPromoCodeRecord
            {
                code = normalizedCode,
                offerId = normalizedOfferId,
                rewardInventoryId = normalizedRewardId,
                pricePaid = price,
                basketCapacityPercent = capacityPercent,
                createdAtUnixSeconds = createdAtUnixSeconds,
                expiresAtUnixSeconds = expiresAtUnixSeconds,
                isRedeemed = false,
                redeemedAtUnixSeconds = 0
            };

        currencyEntry.amount = previousBalance - price;

        accountData.halloweenBasketCapacityUsed =
            previousCapacity + capacityPercent;

        accountData.halloweenPromoCodes.Add(record);

        try
        {
            SaveAccountData();
            return true;
        }
        catch (Exception exception)
        {
            currencyEntry.amount = previousBalance;

            accountData.halloweenBasketCapacityUsed =
                previousCapacity;

            accountData.halloweenPromoCodes.Remove(record);

            Debug.LogError(
                $"[AccountManager] Не удалось сохранить покупку " +
                $"Хеллоуинского обмена: {exception.Message}",
                this);

            failureReason =
                "Не удалось сохранить покупку. " +
                "Конфеты и корзина не должны измениться.";

            return false;
        }
    }

    public bool TryRedeemHalloweenPromoCode(
    string code,
    string expectedRewardInventoryId,
    long nowUnixSeconds,
    out string failureReason)
    {
        failureReason = string.Empty;

        if (accountData == null ||
            accountData.halloweenPromoCodes == null ||
            string.IsNullOrWhiteSpace(code) ||
            string.IsNullOrWhiteSpace(expectedRewardInventoryId))
        {
            return FailHalloweenExchange(
                "Промокод или ID награды не заданы.",
                out failureReason);
        }

        string normalizedCode = code.Trim().ToUpperInvariant();

        string rewardId =
            NormalizeInventoryId(expectedRewardInventoryId);

        HalloweenPromoCodeRecord record =
            accountData.halloweenPromoCodes.Find(item =>
                item != null &&
                string.Equals(
                    item.code,
                    normalizedCode,
                    StringComparison.OrdinalIgnoreCase));

        if (record == null)
            return FailHalloweenExchange(
                "Промокод не найден в текущем аккаунте.",
                out failureReason);

        if (record.isRedeemed)
            return FailHalloweenExchange(
                "Этот промокод уже использован.",
                out failureReason);

        if (nowUnixSeconds >= record.expiresAtUnixSeconds)
            return FailHalloweenExchange(
                "Срок действия промокода истёк.",
                out failureReason);

        if (!InventoryIdsEqual(record.rewardInventoryId, rewardId))
            return FailHalloweenExchange(
                "ID награды не совпадает с данными промокода.",
                out failureReason);

        if (HasUniqueInventoryItem(rewardId))
            return FailHalloweenExchange(
                "Этот уникальный предмет уже есть в инвентаре.",
                out failureReason);

        bool previousRedeemed = record.isRedeemed;
        long previousRedeemedAt = record.redeemedAtUnixSeconds;

        record.isRedeemed = true;
        record.redeemedAtUnixSeconds = nowUnixSeconds;

        accountData.ownedUniqueInventoryItems.Add(rewardId);

        try
        {
            SaveAccountData();
            return true;
        }
        catch (Exception exception)
        {
            record.isRedeemed = previousRedeemed;
            record.redeemedAtUnixSeconds = previousRedeemedAt;

            RemoveUniqueInventoryItem(rewardId);

            Debug.LogError(
                $"[AccountManager] Не удалось сохранить погашение " +
                $"промокода: {exception.Message}",
                this);

            failureReason =
                "Не удалось сохранить награду. Попробуй ещё раз.";

            return false;
        }
    }

        /// <summary>
    /// Test-only reset for the Halloween exchange pool. It removes promo records
    /// related to the supplied offers/rewards and clears basket capacity.
    /// Optionally removes the offer rewards from the owned-unique-item list.
    /// </summary>
    public bool ResetHalloweenExchangeState(
        IReadOnlyList<string> offerIds,
        IReadOnlyList<string> rewardIds,
        bool removeOwnedRewards)
    {
        if (accountData == null)
            return false;

        if (accountData.halloweenPromoCodes == null)
            accountData.halloweenPromoCodes = new List<HalloweenPromoCodeRecord>();
        if (accountData.ownedUniqueInventoryItems == null)
            accountData.ownedUniqueInventoryItems = new List<string>();

        HashSet<string> normalizedOfferIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        HashSet<string> normalizedRewardIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddHalloweenIds(normalizedOfferIds, offerIds);
        AddHalloweenIds(normalizedRewardIds, rewardIds);

        int oldCapacity = accountData.halloweenBasketCapacityUsed;
        List<string> oldOwnedItems = new List<string>(accountData.ownedUniqueInventoryItems);
        List<HalloweenPromoCodeRecord> oldPromoCodes =
            new List<HalloweenPromoCodeRecord>(accountData.halloweenPromoCodes);

        accountData.halloweenBasketCapacityUsed = 0;
        accountData.halloweenPromoCodes.RemoveAll(record =>
            record != null &&
            (ContainsHalloweenId(normalizedOfferIds, record.offerId) ||
             ContainsHalloweenId(normalizedRewardIds, record.rewardInventoryId)));

        if (removeOwnedRewards)
        {
            accountData.ownedUniqueInventoryItems.RemoveAll(id =>
                ContainsHalloweenId(normalizedRewardIds, id));
        }

        try
        {
            SaveAccountData();
            return true;
        }
        catch (Exception exception)
        {
            accountData.halloweenBasketCapacityUsed = oldCapacity;
            accountData.ownedUniqueInventoryItems = oldOwnedItems;
            accountData.halloweenPromoCodes = oldPromoCodes;
            Debug.LogError(
                "[AccountManager] Не удалось сбросить состояние Halloween Exchange: " +
                exception.Message,
                this);
            return false;
        }
    }

    private static void AddHalloweenIds(
        HashSet<string> destination,
        IReadOnlyList<string> ids)
    {
        if (destination == null || ids == null)
            return;

        for (int i = 0; i < ids.Count; i++)
        {
            string normalized = NormalizeInventoryId(ids[i]);
            if (normalized != null)
                destination.Add(normalized);
        }
    }

    private static bool ContainsHalloweenId(HashSet<string> ids, string id)
    {
        string normalized = NormalizeInventoryId(id);
        return normalized != null && ids != null && ids.Contains(normalized);
    }
private static bool FailHalloweenExchange(
        string message,
        out string failureReason)
    {
        failureReason = message;
        return false;
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
