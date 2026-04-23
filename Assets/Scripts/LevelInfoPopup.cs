using UnityEngine;
using UnityEngine.UI;

public class LevelInfoPopup : MonoBehaviour
{
    [SerializeField] private GameObject popupPanel; // Ссылка на панель
    [SerializeField] private Text infoText; // Текст в панели

    private float popupDuration = 5f; // Время отображения окна

    private void Start()
    {
        popupPanel.SetActive(false); // Убедись, что окно скрыто
    }

    public void ShowLevelInfo()
    {
        // Показываем панель
        popupPanel.SetActive(true);

        // Скрываем через 5 секунд
        Invoke(nameof(HideLevelInfo), popupDuration);
    }

    private void HideLevelInfo()
    {
        popupPanel.SetActive(false);
    }
}