using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [SerializeField] private Slider soundSlider; // Слайдер звука
    [SerializeField] private Slider musicSlider; // Слайдер музыки

    public void AdjustSoundVolume(float volume)
    {
        // Меняем громкость звука
        AudioListener.volume = volume;
    }

    public void AdjustMusicVolume(float volume)
    {
        // Пример: связываем с музыкальным микшером
        // AudioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    private void Start()
    {
        // Задаем начальное значение слайдеров
        soundSlider.value = 65f;
        musicSlider.value = 65f; // Пример
    }
}