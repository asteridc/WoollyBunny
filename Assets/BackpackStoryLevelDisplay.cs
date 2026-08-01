using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BackpackStoryLevelDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider experienceProgressSlider;
    [SerializeField] private Image sliderFillImage;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;

    [Header("Colors")]
    [SerializeField] private Color normalFillColor = Color.yellow;
    [SerializeField] private Color levelUpFillColor = Color.green;

    private Coroutine levelUpCoroutine;

    private void Start()
    {
        if (AccountManager.Instance != null)
        {
            AccountManager.Instance.OnStoryExperienceChanged += UpdateDisplay;
            AccountManager.Instance.OnStoryLevelChanged += OnLevelChanged;
            AccountManager.Instance.OnStoryLevelUp += OnLevelUp;
        }

        UpdateDisplay(0);
    }

    private void OnDestroy()
    {
        if (AccountManager.Instance != null)
        {
            AccountManager.Instance.OnStoryExperienceChanged -= UpdateDisplay;
            AccountManager.Instance.OnStoryLevelChanged -= OnLevelChanged;
            AccountManager.Instance.OnStoryLevelUp -= OnLevelUp;
        }

        if (levelUpCoroutine != null)
            StopCoroutine(levelUpCoroutine);
    }

    private void UpdateDisplay(int currentExp)
    {
        if (AccountManager.Instance == null)
            return;

        int level = AccountManager.Instance.GetCurrentLevel();
        float progress = AccountManager.Instance.GetLevelProgressPercent();
        int expUntilNext = AccountManager.Instance.GetExperienceUntilNextLevel();
        int expRequired = AccountManager.Instance.GetExperienceRequiredForNextLevel();

        // Обновляем слайдер
        if (experienceProgressSlider != null)
        {
            experienceProgressSlider.value = progress;
        }

        // Обновляем цвет слайдера
        if (sliderFillImage != null)
        {
            sliderFillImage.color = normalFillColor;
        }

        // Обновляем текст уровня - просто число
        if (levelText != null)
            levelText.text = $"{level}";

        // Обновляем текст опыта - просто число текущего опыта + требуемого за уровень
        if (experienceText != null)
        {
            if (level >= 20)
                experienceText.text = $"МАКС";
            else
                experienceText.text = $"{expUntilNext} / {expRequired} XP";
        }

        Debug.Log($"[BackpackStoryLevelDisplay] Уровень {level}, опыт {currentExp}/{expRequired}, прогресс {progress:P0}");
    }

    private void OnLevelChanged(int newLevel)
    {
        if (levelText != null)
            levelText.text = $"УС: {newLevel}/20";

        Debug.Log($"[Backpack] Уровень Сюжета повышен до {newLevel}");
    }

    private void OnLevelUp()
    {
        // Вспышка зелёного цвета при повышении уровня
        if (levelUpCoroutine != null)
            StopCoroutine(levelUpCoroutine);

        levelUpCoroutine = StartCoroutine(LevelUpFlash());
    }

    private System.Collections.IEnumerator LevelUpFlash()
    {
        if (sliderFillImage == null)
            yield break;

        // Меняем цвет на зелёный
        sliderFillImage.color = levelUpFillColor;
        yield return new WaitForSeconds(0.5f);

        // Возвращаем обратно
        sliderFillImage.color = normalFillColor;
    }
}
