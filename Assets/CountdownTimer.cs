using System;
using TMPro;
using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private TMP_Text countdownText;

    [Header("End Date (UTC)")]
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

    private void UpdateCountdown()
    {
        nextUpdateTime = Time.unscaledTime + 1f;

        if (countdownText == null || !hasValidEndTime)
            return;

        TimeSpan remaining = endTimeUtc - DateTime.UtcNow;
        if (remaining <= TimeSpan.Zero)
        {
            countdownText.text = LanguageManager.CurrentLanguage == Language.English
                ? englishExpiredText
                : russianExpiredText;
            enabled = false;
            return;
        }

        countdownText.text = FormatRemainingTime(remaining, LanguageManager.CurrentLanguage);
    }

    private void OnLanguageChanged(Language language)
    {
        if (countdownText == null || !hasValidEndTime)
            return;

        TimeSpan remaining = endTimeUtc - DateTime.UtcNow;
        countdownText.text = remaining <= TimeSpan.Zero
            ? (language == Language.English ? englishExpiredText : russianExpiredText)
            : FormatRemainingTime(remaining, language);
    }

    private string FormatRemainingTime(TimeSpan remaining, Language language)
    {
        if (language == Language.English)
        {
            if (remaining.TotalDays >= 1)
                return $"{(int)remaining.TotalDays} d {remaining.Hours} h";

            if (remaining.TotalHours >= 1)
                return $"{(int)remaining.TotalHours} h {remaining.Minutes} m";

            return $"{remaining.Minutes} m {remaining.Seconds} s";
        }

        if (remaining.TotalDays >= 1)
            return $"{(int)remaining.TotalDays} дн. {remaining.Hours} ч.";

        if (remaining.TotalHours >= 1)
            return $"{(int)remaining.TotalHours} ч. {remaining.Minutes} мин.";

        return $"{remaining.Minutes} мин. {remaining.Seconds} с.";
    }
}
