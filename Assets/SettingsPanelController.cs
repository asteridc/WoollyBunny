using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI valueText;
    
    [Header("Language")]
    [SerializeField] private TextMeshProUGUI languageLabel;

    [Header("Story Hints")]
    [SerializeField] private TextMeshProUGUI storyHintsOnText;
    [SerializeField] private TextMeshProUGUI storyHintsOffText;

    private const int Step = 1;

    void OnEnable()
    {
        int value = AudioSettings.SFXVolume;
        sfxSlider.SetValueWithoutNotify(value);
        UpdateValueText(value);

        sfxSlider.onValueChanged.AddListener(OnSliderChanged);

        UpdateLanguageLabel();
        UpdateStoryHintsLabel();
    }

    void OnDisable()
    {
        sfxSlider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    public void OnLanguageLeft()
    {
        SwitchLanguage(-1);
    }

    public void OnLanguageRight()
    {
        SwitchLanguage(1);
    }

    public void OnStoryHintsLeft()
    {
        ToggleStoryHints();
    }

    public void OnStoryHintsRight()
    {
        ToggleStoryHints();
    }


    private void SwitchLanguage(int direction)
    {
        int langCount = System.Enum.GetValues(typeof(Language)).Length;
        int currentIndex = (int)LanguageManager.CurrentLanguage;

        int newIndex = (currentIndex + direction + langCount) % langCount;
        LanguageManager.SetLanguage((Language)newIndex);

        UpdateLanguageLabel();
    }

    private void UpdateLanguageLabel()
    {
        languageLabel.text =
            LanguageManager.CurrentLanguage == Language.Russian
            ? "Русский"
            : "English";
    }

    private void ToggleStoryHints()
    {
	    bool isOffShown = storyHintsOffText != null && storyHintsOffText.gameObject.activeSelf;
   	    bool isOnShown = storyHintsOnText != null && storyHintsOnText.gameObject.activeSelf;
    	
    	if (isOffShown)
   	    {
    	    storyHintsOnText?.gameObject.SetActive(true);
    	    storyHintsOffText?.gameObject.SetActive(false);
    	    StoryHintsSettings.EnableHintsFunction();
   	        return;
   	    }

        if (isOnShown)
        {
            storyHintsOnText?.gameObject.SetActive(false);
            storyHintsOffText?.gameObject.SetActive(true);
            StoryHintsSettings.DisableHintsFunction();
            return;
        }

        bool nextState = !StoryHintsSettings.IsEnabled;
        storyHintsOnText?.gameObject.SetActive(nextState);
        storyHintsOffText?.gameObject.SetActive(!nextState);

        if (nextState)
        {
             StoryHintsSettings.EnableHintsFunction();
        }
        else
        {
             StoryHintsSettings.DisableHintsFunction();
        }
    }
   
        private void UpdateStoryHintsLabel()
         {
             bool isOffShown = storyHintsOffText != null && storyHintsOffText.gameObject.activeSelf;
              bool isOnShown = storyHintsOnText != null && storyHintsOnText.gameObject.activeSelf;
   
            if (isOffShown)
                  {
                     StoryHintsSettings.DisableHintsFunction();
                      return;
                 }
   
             if (isOnShown)
                  {
                      StoryHintsSettings.EnableHintsFunction();
                      return;
                  }
    
            bool enabled = StoryHintsSettings.IsEnabled;
             storyHintsOnText?.gameObject.SetActive(enabled);
              storyHintsOffText?.gameObject.SetActive(!enabled);
          }


private void OnLanguageChanged(int index)
    {
        switch (index)
        {
            case 0:
                LanguageManager.SetLanguage(Language.Russian);
                break;
            case 1:
                LanguageManager.SetLanguage(Language.English);
                break;
        }
    }

    private void OnSliderChanged(float value)
    {
        int intValue = Mathf.RoundToInt(value);
        AudioSettings.SFXVolume = intValue;
        UpdateValueText(intValue);

        ApplySFXVolume();
    }

    public void OnPlusPressed()
    {
        SetValue(AudioSettings.SFXVolume + Step);
    }

    public void OnMinusPressed()
    {
        SetValue(AudioSettings.SFXVolume - Step);
    }

    private void SetValue(int value)
    {
        value = Mathf.Clamp(value, 0, 100);
        AudioSettings.SFXVolume = value;

        sfxSlider.SetValueWithoutNotify(value);
        UpdateValueText(value);

        ApplySFXVolume();
    }

    private void UpdateValueText(int value)
    {
        valueText.text = value.ToString();
    }

    private void ApplySFXVolume()
    {
        // временно, пока нет AudioManager
        AudioListener.volume = AudioSettings.SFXVolume / 100f;
    }
}
