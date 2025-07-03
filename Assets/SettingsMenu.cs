using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
    public GameObject settingsPanel; // Ссылка на SettingsPanel

    // Включить настройки
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    // Закрыть настройки
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
}