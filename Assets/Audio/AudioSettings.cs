using UnityEngine;

public static class AudioSettings
{
    public static int SFXVolume
    {
        get => PlayerPrefs.GetInt("SFXVolume", 100);
        set
        {
            value = Mathf.Clamp(value, 0, 100);
            PlayerPrefs.SetInt("SFXVolume", value);
        }
    }
}
