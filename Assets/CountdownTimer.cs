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

    [Header("Expired Text")]
    [SerializeField] private string russianExpiredText = "Событие завершено";
    [SerializeField] private string englishExpiredText = "Event ended";

    private DateTime endTimeUtc;
    private bool hasValidEndTime;
    private bool hasRuntimeTarget;
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
        if (hasRuntimeTarget)
            return;

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
        UpdateCountdown();
    }

    private void UpdateCountdown()
    {
        nextUpdateTime = Time.unscaledTime + 1f;

        if (countdownText == null || !hasValidEndTime)
            return;

        TimeSpan remaining = endTimeUtc - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            countdownText.text = GetExpiredText(LanguageManager.CurrentLanguage);
            return;
        }

        countdownText.text = FormatRemainingTime(
            remaining,
            LanguageManager.CurrentLanguage);
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
