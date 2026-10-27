using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum InventoryEntryType
{
    Currency,
    StackableResource,
    UniqueItem
}

public enum ResourceRarity
{
    Common,
    Rare,
    Epic,
    Relic,
    Unique
}

[Serializable]
public class InventoryEntryDefinition
{
    public string id;
    public InventoryEntryType type;
    public ResourceRarity rarity;
    [TextArea] public string russianName;
    [TextArea] public string englishName;
    public Sprite icon;
    [Min(0)] public int initialAmount;
    public bool initiallyOwned;
    public bool hasExpiration;
    public int expirationYear = 2026;
    [Range(1, 12)] public int expirationMonth = 1;
    [Range(1, 31)] public int expirationDay = 1;
    [Range(0, 23)] public int expirationHour;
    [Range(0, 59)] public int expirationMinute;
    [Range(0, 59)] public int expirationSecond;
}

public class ResourceInventoryManager : MonoBehaviour
{
    [Serializable]
    private class LegacyAmount
    {
        public string id;
        public int amount;
    }

    [Serializable]
    private class LegacyInventorySaveData
    {
        public bool initialized;
        public List<LegacyAmount> amounts = new List<LegacyAmount>();
        public List<string> ownedUniqueItems = new List<string>();
    }

    public static ResourceInventoryManager Instance { get; private set; }

    [Header("Catalog")]
    [SerializeField] private InventoryEntryDefinition[] entries;

    [Header("Unique Items UI")]
    [SerializeField] private Transform uniqueItemsContainer;
    [SerializeField] private ResourceItemView uniqueItemPrefab;

    private readonly Dictionary<string, ResourceItemView> uniqueItemViews = new Dictionary<string, ResourceItemView>();
    private bool accountInventoryInitialized;

    public event Action<string, int> AmountChanged;
    public event Action InventoryChanged;

    // Legacy SavePath removed; legacy resource file is read directly by LoadLegacyInventory for migration.

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeAccountInventory();
        accountInventoryInitialized = AccountManager.Instance != null &&
            AccountManager.Instance.IsInventoryInitialized;
        BuildOwnedUniqueItems();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public InventoryEntryDefinition GetDefinition(string id)
    {
        if (string.IsNullOrWhiteSpace(id) || entries == null)
            return null;

        string normalizedId = id.Trim();
        foreach (InventoryEntryDefinition entry in entries)
        {
            if (entry != null && !string.IsNullOrWhiteSpace(entry.id) &&
                string.Equals(entry.id.Trim(), normalizedId, StringComparison.OrdinalIgnoreCase))
                return entry;
        }

        return null;
    }

    public int GetAmount(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null)
            return 0;

        string entryId = definition.id.Trim();
        if (definition.type == InventoryEntryType.UniqueItem)
            return IsOwned(entryId) ? 1 : 0;

        return AccountManager.Instance != null
            ? AccountManager.Instance.GetInventoryAmount(entryId)
            : 0;
    }

    public bool IsOwned(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || definition.type != InventoryEntryType.UniqueItem)
            return false;

        return AccountManager.Instance != null &&
            AccountManager.Instance.HasUniqueInventoryItem(definition.id.Trim());
    }

    public bool CanUse(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || IsExpired(id))
            return false;

        return definition.type == InventoryEntryType.UniqueItem
            ? IsOwned(id)
            : GetAmount(id) > 0;
    }

    public bool IsExpired(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || !definition.hasExpiration)
            return false;

        try
        {
            DateTime expiryUtc = new DateTime(
                definition.expirationYear,
                definition.expirationMonth,
                definition.expirationDay,
                definition.expirationHour,
                definition.expirationMinute,
                definition.expirationSecond,
                DateTimeKind.Utc);

            return DateTime.UtcNow >= expiryUtc;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Debug.LogError($"ResourceInventoryManager: некорректный UTC срок ресурса '{id}': {exception.Message}", this);
            return true;
        }
    }

    public bool SetAmount(string id, int amount)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || definition.type == InventoryEntryType.UniqueItem ||
            amount < 0 || IsExpired(id))
            return false;

        string entryId = definition.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        if (!accountManager.SetInventoryAmount(entryId, amount))
            return false;

        AmountChanged?.Invoke(entryId, amount);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool ExchangeUniqueItem(string fromId, string toId)
    {
        if (!CanUse(fromId))
            return false;

        InventoryEntryDefinition source = GetDefinition(fromId);
        InventoryEntryDefinition target = GetDefinition(toId);
        if (source == null || target == null ||
            source.type != InventoryEntryType.UniqueItem ||
            target.type != InventoryEntryType.UniqueItem || IsExpired(toId) ||
            IsOwned(toId) || IdsEqual(source.id, target.id))
            return false;

        string sourceId = source.id.Trim();
        string targetId = target.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        if (!accountManager.ExchangeUniqueInventoryItems(sourceId, targetId))
            return false;

        if (uniqueItemViews.TryGetValue(sourceId, out ResourceItemView oldView))
        {
            uniqueItemViews.Remove(sourceId);
            if (oldView != null)
                Destroy(oldView.gameObject);
        }

        CreateUniqueItemView(target);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveUniqueItem(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || definition.type != InventoryEntryType.UniqueItem ||
            IsExpired(id) || !IsOwned(id))
            return false;

        string entryId = definition.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        if (!accountManager.SetUniqueInventoryItemOwned(entryId, false))
            return false;

        if (uniqueItemViews.TryGetValue(entryId, out ResourceItemView view))
        {
            uniqueItemViews.Remove(entryId);
            if (view != null)
                Destroy(view.gameObject);
        }

        InventoryChanged?.Invoke();
        return true;
    }

    public bool Add(string id, int amount = 1)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || amount <= 0 || IsExpired(id) ||
            definition.type == InventoryEntryType.UniqueItem)
            return false;

        string entryId = definition.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        int newAmount = accountManager.GetInventoryAmount(entryId) + amount;
        if (!accountManager.SetInventoryAmount(entryId, newAmount))
            return false;

        AmountChanged?.Invoke(entryId, newAmount);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool Spend(string id, int amount = 1)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || amount <= 0 || IsExpired(id) ||
            definition.type == InventoryEntryType.UniqueItem)
            return false;

        string entryId = definition.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        int currentAmount = accountManager.GetInventoryAmount(entryId);
        if (currentAmount < amount)
            return false;

        int newAmount = currentAmount - amount;
        if (!accountManager.SetInventoryAmount(entryId, newAmount))
            return false;

        AmountChanged?.Invoke(entryId, newAmount);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool Exchange(string fromId, int fromAmount, string toId, int toAmount)
    {
        if (fromAmount <= 0 || toAmount <= 0)
            return false;

        InventoryEntryDefinition from = GetDefinition(fromId);
        InventoryEntryDefinition to = GetDefinition(toId);
        if (from == null || to == null || from.type == InventoryEntryType.UniqueItem ||
            to.type == InventoryEntryType.UniqueItem || IsExpired(fromId) || IsExpired(toId) ||
            GetAmount(fromId) < fromAmount || IdsEqual(from.id, to.id))
            return false;

        string sourceId = from.id.Trim();
        string targetId = to.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        if (!accountManager.ExchangeInventoryAmounts(
                sourceId,
                fromAmount,
                targetId,
                toAmount))
            return false;

        int fromBalance = accountManager.GetInventoryAmount(sourceId);
        int toBalance = accountManager.GetInventoryAmount(targetId);

        AmountChanged?.Invoke(sourceId, fromBalance);
        AmountChanged?.Invoke(targetId, toBalance);
        InventoryChanged?.Invoke();
        return true;
    }

    /// <summary>
    /// Сообщает UI, что обычный баланс был изменён
    /// внешней транзакцией в AccountManager.
    /// </summary>
    public void NotifyAccountAmountChanged(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);

        if (definition == null ||
            definition.type == InventoryEntryType.UniqueItem)
            return;

        AmountChanged?.Invoke(
            definition.id.Trim(),
            GetAmount(definition.id));

        InventoryChanged?.Invoke();
    }

    /// <summary>
    /// Обновляет отображение уникального предмета,
    /// который был добавлен через AccountManager.
    /// Не записывает предмет повторно в сохранение.
    /// </summary>
    public void RefreshUniqueItemViewAfterExternalGrant(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);

        if (definition != null &&
            definition.type == InventoryEntryType.UniqueItem &&
            IsOwned(definition.id))
        {
            CreateUniqueItemView(definition);
            ConfigureExpiryTimer(definition);
        }

        InventoryChanged?.Invoke();
    }

    public bool GrantReward(string id, int amount)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null)
            return false;

        string entryId = definition.id.Trim();
        if (IsExpired(entryId))
            return false;

        if (definition.type == InventoryEntryType.UniqueItem)
            return amount == 1 && GrantUniqueItem(entryId);

        return Add(entryId, amount);
    }

    [Obsolete("Use GrantReward instead")]
    public bool GrantCustomReward(string rewardId, int amount)
    {
        return GrantReward(rewardId, amount);
    }

    public bool TryCollectPumpkinLantern(
        string id,
        int amount,
        long nextAccrualUnixTimestamp,
        out int newAmount)
    {
        newAmount = 0;
        InventoryEntryDefinition definition = GetDefinition(id);

        if (definition == null ||
            definition.type == InventoryEntryType.UniqueItem ||
            amount <= 0 ||
            IsExpired(id))
            return false;

        AccountManager accountManager = AccountManager.GetOrCreate();
        string entryId = definition.id.Trim();

        if (!accountManager.TryCollectPumpkinLantern(
                entryId,
                amount,
                nextAccrualUnixTimestamp,
                out newAmount))
            return false;

        AmountChanged?.Invoke(entryId, newAmount);
        InventoryChanged?.Invoke();
        return true;
    }

    public bool GrantUniqueItem(string id)
    {
        InventoryEntryDefinition definition = GetDefinition(id);
        if (definition == null || definition.type != InventoryEntryType.UniqueItem || IsExpired(id))
            return false;

        string entryId = definition.id.Trim();
        AccountManager accountManager = AccountManager.GetOrCreate();
        if (!accountManager.SetUniqueInventoryItemOwned(entryId, true))
            return false;

        CreateUniqueItemView(definition);
        InventoryChanged?.Invoke();
        return true;
    }

    public void BindDisplay(ResourceAmountDisplay display)
    {
        if (display != null)
            display.Refresh(GetDefinition(display.EntryId), GetAmount(display.EntryId), IsExpired(display.EntryId));
    }

    public void RegisterInventoryUI(ResourceInventoryUI inventoryUI)
    {
        if (inventoryUI == null)
            return;

        uniqueItemsContainer = inventoryUI.UniqueItemsContainer;
        uniqueItemPrefab = inventoryUI.UniqueItemPrefab;
        BuildOwnedUniqueItems();

        if (entries != null)
        {
            foreach (InventoryEntryDefinition entry in entries)
            {
                if (entry != null && entry.hasExpiration)
                    ConfigureExpiryTimer(entry);
            }
        }

        ResourceAmountDisplay[] displays = FindObjectsOfType<ResourceAmountDisplay>(true);
        foreach (ResourceAmountDisplay display in displays)
            display.Bind(this);
    }

    private void InitializeAccountInventory()
    {
        if (AccountManager.Instance == null)
        {
            Debug.LogError("ResourceInventoryManager: AccountManager отсутствует.", this);
            return;
        }

        bool initialized = AccountManager.Instance.IsInventoryInitialized;
        LegacyInventorySaveData legacyData = initialized ? null : LoadLegacyInventory();
        if (entries != null)
        {
            foreach (InventoryEntryDefinition entry in entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.id))
                    continue;

                if (!initialized)
                {
                    if (entry.type == InventoryEntryType.UniqueItem)
                    {
                        bool wasOwned = legacyData != null && legacyData.ownedUniqueItems != null &&
                            legacyData.ownedUniqueItems.Exists(item => IdsEqual(item, entry.id));
                        if (wasOwned || entry.initiallyOwned)
                            AccountManager.Instance.SetUniqueInventoryItemOwned(entry.id, true);
                    }
                    else
                    {
                        LegacyAmount oldAmount = legacyData?.amounts?.Find(
                            item => item != null && IdsEqual(item.id, entry.id));
                        int amount = oldAmount != null
                            ? Mathf.Max(0, oldAmount.amount)
                            : Mathf.Max(0, entry.initialAmount);
                        AccountManager.Instance.SetInventoryAmount(entry.id, amount);
                    }
                }
                else if (entry.type != InventoryEntryType.UniqueItem &&
                    !HasAccountBalance(entry.id))
                    AccountManager.Instance.SetInventoryAmount(entry.id, 0);
            }
        }

        AccountManager.Instance.MarkInventoryInitialized();
    }

    private LegacyInventorySaveData LoadLegacyInventory()
    {
        string path = Path.Combine(Application.persistentDataPath, "resource_inventory.json");
        if (!File.Exists(path))
            return null;

        try
        {
            return JsonUtility.FromJson<LegacyInventorySaveData>(File.ReadAllText(path));
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"ResourceInventoryManager: не удалось перенести старый файл инвентаря: {exception.Message}", this);
            return null;
        }
    }

    private bool HasAccountBalance(string id)
    {
        return AccountManager.Instance.GetInventoryAmount(id) > 0 ||
            AccountManager.Instance.HasInventoryAmountRecord(id);
    }

    private static bool IdsEqual(string first, string second)
    {
        return !string.IsNullOrWhiteSpace(first) && !string.IsNullOrWhiteSpace(second) &&
            string.Equals(first.Trim(), second.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    private void BuildOwnedUniqueItems()
    {
        if (uniqueItemsContainer == null || uniqueItemPrefab == null)
            return;

        if (AccountManager.Instance == null || entries == null)
            return;

        foreach (InventoryEntryDefinition definition in entries)
        {
            if (definition != null && definition.type == InventoryEntryType.UniqueItem &&
                IsOwned(definition.id))
                CreateUniqueItemView(definition);
        }
    }

    private void CreateUniqueItemView(InventoryEntryDefinition definition)
    {
        if (uniqueItemsContainer == null || uniqueItemPrefab == null ||
            uniqueItemViews.ContainsKey(definition.id))
            return;

        ResourceItemView view = Instantiate(uniqueItemPrefab, uniqueItemsContainer);
        view.Bind(definition, 1, IsExpired(definition.id));
        uniqueItemViews.Add(definition.id, view);
        ConfigureExpiryTimer(definition);
    }

    private DateTime GetExpiryUtc(InventoryEntryDefinition definition)
    {
        return new DateTime(
            definition.expirationYear,
            definition.expirationMonth,
            definition.expirationDay,
            definition.expirationHour,
            definition.expirationMinute,
            definition.expirationSecond,
            DateTimeKind.Utc);
    }

    private void ConfigureExpiryTimer(InventoryEntryDefinition definition)
    {
        if (!definition.hasExpiration)
            return;

        try
        {
            DateTime expiryUtc = GetExpiryUtc(definition);
            CountdownTimer[] timers = FindObjectsOfType<CountdownTimer>(true);
            foreach (CountdownTimer timer in timers)
            {
                if (timer.TimerId == "ResourceExpiry:" + definition.id)
                    timer.SetTargetUtc(expiryUtc);
            }
        }
        catch (ArgumentOutOfRangeException exception)
        {
            Debug.LogError($"ResourceInventoryManager: некорректная дата срока '{definition.id}': {exception.Message}", this);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!accountInventoryInitialized && AccountManager.Instance != null)
        {
            InitializeAccountInventory();
            accountInventoryInitialized = true;
        }

        ResourceAmountDisplay[] displays = FindObjectsOfType<ResourceAmountDisplay>(true);
        foreach (ResourceAmountDisplay display in displays)
            display.Bind(this);
        uniqueItemViews.Clear();

        uniqueItemsContainer = null;
        uniqueItemPrefab = null;
        ResourceInventoryUI[] inventoryUis = FindObjectsOfType<ResourceInventoryUI>(true);
        if (inventoryUis.Length > 0)
        {
            uniqueItemsContainer = inventoryUis[0].UniqueItemsContainer;
            uniqueItemPrefab = inventoryUis[0].UniqueItemPrefab;
            BuildOwnedUniqueItems();
        }

        InventoryChanged?.Invoke();

        if (entries != null)
        {
            foreach (InventoryEntryDefinition entry in entries)
            {
                if (entry != null && entry.hasExpiration)
                    ConfigureExpiryTimer(entry);
            }
        }
    }

}
