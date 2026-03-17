using System;
using UnityEngine;

public static class StoryHintsSettings
{
    private const string Key = "StoryHintsEnabled";

    public static event Action<bool> OnStoryHintsChanged;

    public static bool IsEnabled
    {
        get => PlayerPrefs.GetInt(Key, 0) == 1;
        private set
        {
            PlayerPrefs.SetInt(Key, value ? 1 : 0);
            PlayerPrefs.Save();
            OnStoryHintsChanged?.Invoke(value);
        }
    }

    public static void EnableHintsFunction()
    {
        if (IsEnabled)
        {
            return;
        }

        IsEnabled = true;
    }

    public static void DisableHintsFunction()
    {
        if (!IsEnabled)
        {
            return;
        }

        IsEnabled = false;
    }
}