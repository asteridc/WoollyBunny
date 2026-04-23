using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel; // Панель настроек

    public void ToggleSettingsPanel()
    {
        // Переключение видимости панели
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
}