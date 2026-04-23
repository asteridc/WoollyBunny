using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatisticsChanging : MonoBehaviour
{
    // Переменные прогресса критериев
    public int storyProgress = 0; // Сюжетные главы
    public int sideQuestsProgress = 0; // Доп квесты
    public int cachesFound = 0; // Тайники
    public int campInteractions = 0; // Разговоры и костры
    public int miscellaneousProgress = 0; // Разное

    // Макс значения
    private const int maxStoryProgress = 12;
    private const int maxSideQuestsProgress = 25;
    private const int maxCachesFound = 20;
    private const int maxCampInteractions = 43;
    private const int maxMiscellaneousProgress = 16;

    // UI элементы
    public TextMeshProUGUI storyProgressText;
    public TextMeshProUGUI sideQuestsProgressText;
    public TextMeshProUGUI cachesFoundText;
    public TextMeshProUGUI campInteractionsText;
    public TextMeshProUGUI miscellaneousProgressText;

    public Image storyProgressBar;
    public Image sideQuestsProgressBar;
    public Image cachesFoundBar;
    public Image campInteractionsBar;
    public Image miscellaneousProgressBar;

    private void Start()
    {
        // Загрузка сохраненного прогресса
        LoadProgress();
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Обновление текста прогресса
        storyProgressText.text = $"{storyProgress} / {maxStoryProgress}";
        sideQuestsProgressText.text = $"{sideQuestsProgress} / {maxSideQuestsProgress}";
        cachesFoundText.text = $"{cachesFound} / {maxCachesFound}";
        campInteractionsText.text = $"{campInteractions} / {maxCampInteractions}";
        miscellaneousProgressText.text = $"{miscellaneousProgress} / {maxMiscellaneousProgress}";

        // Обновление шкал
        storyProgressBar.fillAmount = (float)storyProgress / maxStoryProgress;
        sideQuestsProgressBar.fillAmount = (float)sideQuestsProgress / maxSideQuestsProgress;
        cachesFoundBar.fillAmount = (float)cachesFound / maxCachesFound;
        campInteractionsBar.fillAmount = (float)campInteractions / maxCampInteractions;
        miscellaneousProgressBar.fillAmount = (float)miscellaneousProgress / maxMiscellaneousProgress;
    }

    public void UpdateProgress(string category, int value)
    {
        switch (category)
        {
            case "Story":
                storyProgress = Mathf.Clamp(storyProgress + value, 0, maxStoryProgress);
                break;
            case "SideQuests":
                sideQuestsProgress = Mathf.Clamp(sideQuestsProgress + value, 0, maxSideQuestsProgress);
                break;
            case "Caches":
                cachesFound = Mathf.Clamp(cachesFound + value, 0, maxCachesFound);
                break;
            case "CampInteractions":
                campInteractions = Mathf.Clamp(campInteractions + value, 0, maxCampInteractions);
                break;
            case "Miscellaneous":
                miscellaneousProgress = Mathf.Clamp(miscellaneousProgress + value, 0, maxMiscellaneousProgress);
                break;
        }

        SaveProgress();
        UpdateUI();
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt("StoryProgress", storyProgress);
        PlayerPrefs.SetInt("SideQuestsProgress", sideQuestsProgress);
        PlayerPrefs.SetInt("CachesFound", cachesFound);
        PlayerPrefs.SetInt("CampInteractions", campInteractions);
        PlayerPrefs.SetInt("MiscellaneousProgress", miscellaneousProgress);
        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        storyProgress = PlayerPrefs.GetInt("StoryProgress", 0);
        sideQuestsProgress = PlayerPrefs.GetInt("SideQuestsProgress", 0);
        cachesFound = PlayerPrefs.GetInt("CachesFound", 0);
        campInteractions = PlayerPrefs.GetInt("CampInteractions", 0);
        miscellaneousProgress = PlayerPrefs.GetInt("MiscellaneousProgress", 0);
    }
}