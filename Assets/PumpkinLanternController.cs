using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Offline-friendly Halloween lantern. Progress is calculated from UTC timestamps
/// saved in AccountData; candy is granted through ResourceInventoryManager.
/// </summary>
[DefaultExecutionOrder(100)]
public sealed class PumpkinLanternController : MonoBehaviour
{
    [Header("Resource")]
    [SerializeField] private string resourceId = "Конфетыквы";

    [Header("Accumulation")]
    [SerializeField, Min(1)] private int candiesPerInterval = 1;
    [SerializeField, Min(1)] private int intervalMinutes = 15;
    [SerializeField, Min(1)] private int maximumStoredHours = 8;

    [Header("UI")]
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private Button collectButton;
    [SerializeField] private TMP_Text collectButtonText;

    [Header("Localized UI Text")]
    [SerializeField] private string russianCollectText = "Забрать конфеты";
    [SerializeField] private string englishCollectText = "Collect candy";
    [SerializeField] private string russianWaitingFormat = "Следующие конфеты через {0}";
    [SerializeField] private string englishWaitingFormat = "Next candy in {0}";
    [SerializeField] private string russianReadyText = "Фонарь полон! Забери конфеты.";
    [SerializeField] private string englishReadyText = "The lantern is full! Collect your candy.";
    [SerializeField] private string russianUnavailableText = "Фонарь пока недоступен";
    [SerializeField] private string englishUnavailableText = "The lantern is unavailable";
    [SerializeField] private string russianExpiredText = "Событие завершено";
    [SerializeField] private string englishExpiredText = "Event ended";

    private const long UnixTimestampMinimum = 1;
    private float nextUiRefreshTime;

    private long IntervalSeconds => Math.Max(1L, (long)intervalMinutes * 60L);
    private long MaximumCycleSeconds => Math.Max(1L, (long)maximumStoredHours * 3600L);
    private long MaximumIntervals => Math.Max(1L, MaximumCycleSeconds / IntervalSeconds);
    private int MaximumStoredAmount
    {
        get
        {
            long amount = MaximumIntervals * Math.Max(1, candiesPerInterval);
            return amount > int.MaxValue ? int.MaxValue : (int)amount;
        }
    }

    private void OnEnable()
    {
        if (collectButton != null)
            collectButton.onClick.AddListener(Collect);

        LanguageManager.OnLanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        if (collectButton != null)
            collectButton.onClick.RemoveListener(Collect);

        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Start()
    {
        AccountManager account = AccountManager.GetOrCreate();
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (account != null && !account.InitializePumpkinLanternIfNeeded(now))
            Debug.LogError("[PumpkinLantern] Не удалось инициализировать таймер фонаря.", this);

        if (ResourceInventoryManager.Instance == null)
        {
            Debug.LogError(
                "[PumpkinLantern] ResourceInventoryManager не найден. Проверь сцену и каталог ресурсов.",
                this);
        }

        RefreshUI();
    }

    private void Update()
    {
        // The timer is display-only; calculations always use UTC wall-clock time.
        // Do not write a save file every frame.
        if (Time.unscaledTime < nextUiRefreshTime)
            return;

        RefreshUI();
    }

    /// <summary>
    /// Collects the currently available candy once. Partial intervals are preserved,
    /// while reaching the storage cap starts a fresh cycle at collection time.
    /// </summary>
    public void Collect()
    {
        if (AccountManager.Instance == null || ResourceInventoryManager.Instance == null)
        {
            Debug.LogError("[PumpkinLantern] Невозможно собрать конфеты: менеджеры не инициализированы.", this);
            RefreshUI();
            return;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long lastAccrual = AccountManager.Instance.GetPumpkinLanternLastAccrualUnixSeconds();

        if (lastAccrual < UnixTimestampMinimum)
        {
            if (!AccountManager.Instance.InitializePumpkinLanternIfNeeded(now))
                Debug.LogError("[PumpkinLantern] Не удалось сохранить начальное время фонаря.", this);

            RefreshUI();
            return;
        }

        AccrualState state = CalculateAccrual(lastAccrual, now);
        if (state.availableAmount <= 0)
        {
            RefreshUI();
            return;
        }

        // At the cap, the next cycle begins when the player collects.
        // Before the cap, advance by whole intervals to preserve leftover seconds.
        long nextAccrual = state.isFull
            ? now
            : lastAccrual + state.elapsedIntervals * IntervalSeconds;

        if (!ResourceInventoryManager.Instance.TryCollectPumpkinLantern(
                resourceId,
                state.availableAmount,
                nextAccrual,
                out int newBalance))
        {
            Debug.LogWarning(
                $"[PumpkinLantern] Сбор не выполнен. Проверь ресурс '{resourceId}', срок события и сохранение аккаунта.",
                this);
            RefreshUI();
            return;
        }

        Debug.Log(
            $"[PumpkinLantern] Собрано {state.availableAmount} Конфетыкв. " +
            $"Баланс аккаунта: {newBalance}.",
            this);

        RefreshUI();
    }

    private void RefreshUI()
    {
        nextUiRefreshTime = Time.unscaledTime + 0.5f;

        if (collectButtonText != null)
        {
            collectButtonText.text = LanguageManager.CurrentLanguage == Language.English
                ? englishCollectText
                : russianCollectText;
        }

        long lastAccrual = AccountManager.Instance != null
            ? AccountManager.Instance.GetPumpkinLanternLastAccrualUnixSeconds()
            : 0;

        ResourceInventoryManager inventory = ResourceInventoryManager.Instance;
        InventoryEntryDefinition definition = inventory != null
            ? inventory.GetDefinition(resourceId)
            : null;

        if (definition == null)
        {
            SetUI(0, MaximumStoredAmount, false,
                LanguageManager.CurrentLanguage == Language.English
                    ? englishUnavailableText
                    : russianUnavailableText);
            return;
        }

        if (inventory.IsExpired(resourceId))
        {
            int accrued = 0;
            if (lastAccrual >= UnixTimestampMinimum)
            {
                long nowExpired = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                accrued = CalculateAccrual(lastAccrual, nowExpired).availableAmount;
            }

            SetUI(accrued, MaximumStoredAmount, false,
                LanguageManager.CurrentLanguage == Language.English
                    ? englishExpiredText
                    : russianExpiredText);
            return;
        }

        if (lastAccrual < UnixTimestampMinimum)
        {
            SetUI(0, MaximumStoredAmount, false,
                LanguageManager.CurrentLanguage == Language.English
                    ? englishUnavailableText
                    : russianUnavailableText);
            return;
        }

        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        AccrualState state = CalculateAccrual(lastAccrual, now);
        bool canCollect = state.availableAmount > 0;

        string status;
        if (state.isFull)
        {
            status = LanguageManager.CurrentLanguage == Language.English
                ? englishReadyText
                : russianReadyText;
        }
        else
        {
            long secondsUntilNext = IntervalSeconds - (state.elapsedSeconds % IntervalSeconds);
            string time = FormatDuration(secondsUntilNext);
            status = LanguageManager.CurrentLanguage == Language.English
                ? string.Format(englishWaitingFormat, time)
                : string.Format(russianWaitingFormat, time);
        }

        SetUI(state.availableAmount, MaximumStoredAmount, canCollect, status);
    }

    private AccrualState CalculateAccrual(long lastAccrual, long now)
    {
        long elapsed = Math.Max(0L, now - lastAccrual);
        long wholeIntervals = elapsed / IntervalSeconds;
        bool isFull = wholeIntervals >= MaximumIntervals;
        long cappedIntervals = Math.Min(wholeIntervals, MaximumIntervals);
        long rawAmount = cappedIntervals * Math.Max(1, candiesPerInterval);
        int amount = rawAmount > MaximumStoredAmount ? MaximumStoredAmount : (int)rawAmount;

        return new AccrualState
        {
            elapsedSeconds = elapsed,
            elapsedIntervals = cappedIntervals,
            availableAmount = amount,
            isFull = isFull
        };
    }

    private void SetUI(int available, int maximum, bool canCollect, string status)
    {
        if (amountText != null)
            amountText.text = $"{available} / {maximum}";

        if (statusText != null)
            statusText.text = status;

        if (progressSlider != null)
        {
            progressSlider.minValue = 0;
            progressSlider.maxValue = Math.Max(1, maximum);
            progressSlider.value = Mathf.Clamp(available, 0, maximum);
        }

        if (collectButton != null)
            collectButton.interactable = canCollect;
    }

    private static string FormatDuration(long totalSeconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Math.Max(0L, totalSeconds));
        if (time.TotalHours >= 1)
            return $"{(int)time.TotalHours:00}:{time.Minutes:00}:{time.Seconds:00}";

        return $"{time.Minutes:00}:{time.Seconds:00}";
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshUI();
    }

    private struct AccrualState
    {
        public long elapsedSeconds;
        public long elapsedIntervals;
        public int availableAmount;
        public bool isFull;
    }
}
