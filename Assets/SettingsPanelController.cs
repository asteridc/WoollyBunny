using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanelController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI valueText;

    private const int Step = 1;

    void OnEnable()
    {
        int value = AudioSettings.SFXVolume;
        sfxSlider.SetValueWithoutNotify(value);
        UpdateValueText(value);

        sfxSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnDisable()
    {
        sfxSlider.onValueChanged.RemoveListener(OnSliderChanged);
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
