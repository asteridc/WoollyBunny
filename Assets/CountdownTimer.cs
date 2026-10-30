using System;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [Header("Timer Id")]
    [SerializeField] private string timerId = "default";

    [Header("Target")]
    [SerializeField] private TMP_Text countdownText;

    public string TimerId => timerId;

    [Header("End Date and Time (UTC)")]
    [SerializeField] private int year = 2026;
    [SerializeField, Range(1, 12)] private int month = 1;
    [SerializeField, Range(1, 31)] private int day = 1;
    [SerializeField, Range(0, 23)] private int hour;
    [SerializeField, Range(0, 59)] private int minute;
    [SerializeField, Range(0, 59)] private int second;

    [Header("Expired UI")]
    [Tooltip("Эти GameObject будут отключены, когда установленная дата наступит.")]
    [SerializeField] private GameObject[] objectsToHideOnExpiry;

    [Header("Expired Text")]
    [SerializeField] private string russianExpiredText = "Событие завершено";
    [SerializeField] private string englishExpiredText = "Event ended";

    private DateTime endTimeUtc;
    private bool hasValidEndTime;
    private bool hasRuntimeTarget;
    private bool hasInitializedEndTime;
    private bool hasAppliedExpiry;
    private float nextUpdateTime;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;

        InitializeEndTime();
        UpdateCountdown();
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Update()
    {
        if (Time.unscaledTime < nextUpdateTime)
            return;

        UpdateCountdown();
    }

    private void InitializeEndTime()
    {
        if (hasRuntimeTarget || hasInitializedEndTime)
            return;

        hasInitializedEndTime = true;

        try
        {
            endTimeUtc = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
            hasValidEndTime = true;
        }
        catch (ArgumentOutOfRangeException exception)
        {
            hasValidEndTime = false;
            Debug.LogError($"CountdownTimer: некорректная дата/время UTC. {exception.Message}", this);
        }
    }

    /// <summary>
    /// Устанавливает дату окончания во время выполнения.
    /// Если таймер уже завершился, ранее скрытые элементы будут показаны снова.
    /// </summary>
    public void SetTargetUtc(DateTime targetUtc)
    {
        endTimeUtc = targetUtc.Kind == DateTimeKind.Utc
            ? targetUtc
            : targetUtc.ToUniversalTime();

        year = endTimeUtc.Year;
        month = endTimeUtc.Month;
        day = endTimeUtc.Day;
        hour = endTimeUtc.Hour;
        minute = endTimeUtc.Minute;
        second = endTimeUtc.Second;

        hasValidEndTime = true;
        hasRuntimeTarget = true;
        hasInitializedEndTime = true;

        if (hasAppliedExpiry)
            RestoreExpiredObjects();

        UpdateCountdown();
    }

    /// <summary>
    /// Returns the currently configured UTC deadline. Initializes the Inspector date
    /// on demand, so other systems can safely read it even before the first UI update.
    /// </summary>
    public bool TryGetTargetUtc(out DateTime targetUtc)
    {
        InitializeEndTime();
        targetUtc = endTimeUtc;
        return hasValidEndTime;
    }

    private void UpdateCountdown()
    {
        nextUpdateTime = Time.unscaledTime + 1f;

        // Скрытие UI не должно зависеть от наличия текста таймера.
        if (!hasValidEndTime)
            return;

        TimeSpan remaining = endTimeUtc - DateTime.UtcNow;

        if (remaining <= TimeSpan.Zero)
        {
            if (countdownText != null)
                countdownText.text = GetExpiredText(LanguageManager.CurrentLanguage);

            HideExpiredObjects();
            return;
        }

        if (countdownText == null)
            return;

        countdownText.text = FormatRemainingTime(
            remaining,
            LanguageManager.CurrentLanguage);
    }

    private void HideExpiredObjects()
    {
        hasAppliedExpiry = true;

        if (objectsToHideOnExpiry == null)
            return;

        foreach (GameObject target in objectsToHideOnExpiry)
        {
            // Повторно выключаем элемент, если другой скрипт случайно включил его обратно.
            if (target != null && target.activeSelf)
                target.SetActive(false);
        }
    }

    private void RestoreExpiredObjects()
    {
        if (objectsToHideOnExpiry != null)
        {
            foreach (GameObject target in objectsToHideOnExpiry)
            {
                if (target != null)
                    target.SetActive(true);
            }
        }

        hasAppliedExpiry = false;
    }

    private void OnLanguageChanged(Language language)
    {
        UpdateCountdown();
    }

    private string GetExpiredText(Language language)
    {
        return language == Language.English
            ? englishExpiredText
            : russianExpiredText;
    }

    private string FormatRemainingTime(TimeSpan remaining, Language language)
    {
        bool isEnglish = language == Language.English;

        if (remaining.TotalDays >= 1)
        {
            return isEnglish
                ? $"{(int)remaining.TotalDays} d {remaining.Hours} h"
                : $"{(int)remaining.TotalDays} дн. {remaining.Hours} ч.";
        }

        if (remaining.TotalHours >= 1)
        {
            int hours = (int)remaining.TotalHours;
            return isEnglish
                ? $"{hours} h {remaining.Minutes} m"
                : $"{hours} ч. {remaining.Minutes} мин.";
        }

        if (remaining.TotalMinutes >= 1)
        {
            return isEnglish
                ? $"{remaining.Minutes} m {remaining.Seconds} s"
                : $"{remaining.Minutes} мин. {remaining.Seconds} с.";
        }

        int seconds = Mathf.CeilToInt((float)remaining.TotalSeconds);
        return isEnglish
            ? $"{seconds} s"
            : $"{seconds} с.";
    }
}
