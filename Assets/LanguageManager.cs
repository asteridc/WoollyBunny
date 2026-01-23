using System;
using TMPro;
using UnityEngine;

public enum Language
{
    Russian,
    English
}

public static class LanguageManager
{
    public static Language CurrentLanguage { get; private set; }

    public static event Action<Language> OnLanguageChanged;

    private const string LanguageKey = "LANGUAGE";

    static LanguageManager()
    {
        LoadLanguage();
    }

    public static void SetLanguage(Language language)
    {
        if (CurrentLanguage == language) return;

        CurrentLanguage = language;
        PlayerPrefs.SetInt(LanguageKey, (int)language);
        PlayerPrefs.Save();

        OnLanguageChanged?.Invoke(language);
    }

    private static void LoadLanguage()
    {
        if (PlayerPrefs.HasKey(LanguageKey))
        {
            CurrentLanguage = (Language)PlayerPrefs.GetInt(LanguageKey);
        }
        else
        {
            CurrentLanguage = DetectLanguageBySystem();
            PlayerPrefs.SetInt(LanguageKey, (int)CurrentLanguage);
        }
    }

    private static Language DetectLanguageBySystem()
    {
        var systemLang = Application.systemLanguage;

        if (systemLang == SystemLanguage.Russian ||
            systemLang == SystemLanguage.Ukrainian ||
            systemLang == SystemLanguage.Belarusian)
            return Language.Russian;

        return Language.English;
    }
}