using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BackpackStoryLevelDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider experienceProgressSlider;
    [SerializeField] private Image sliderFillImage;

    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private TextMeshProUGUI requiredExperienceText;

    [Header("Colors")]
    [SerializeField] private Color normalFillColor = Color.yellow;
    [SerializeField] private Color levelUpFillColor = Color.green;

    private Coroutine levelUpCoroutine;
    private AccountManager accountManager;

    private void Start()
    {
        accountManager = AccountManager.GetOrCreate();

        accountManager.OnStoryExperienceChanged += HandleExperienceChanged;
        accountManager.OnStoryLevelChanged += HandleLevelChanged;
        accountManager.OnStoryLevelUp += HandleLevelUp;

        Refresh();
    }

    private void OnEnable()
    {
        // Рюкзак может открываться после изменения уровня.
        if (AccountManager.Instance != null)
            Refresh();
    }

    private void OnDestroy()
    {
        if (accountManager != null)
        {
            accountManager.OnStoryExperienceChanged -= HandleExperienceChanged;
            accountManager.OnStoryLevelChanged -= HandleLevelChanged;
            accountManager.OnStoryLevelUp -= HandleLevelUp;
        }

        if (levelUpCoroutine != null)
            StopCoroutine(levelUpCoroutine);
    }

    private void HandleExperienceChanged(int _)
    {
        Refresh();
    }

    private void HandleLevelChanged(int _)
    {
        Refresh();
    }

    private void HandleLevelUp()
    {
        Refresh();

        if (levelUpCoroutine != null)
            StopCoroutine(levelUpCoroutine);

        levelUpCoroutine = StartCoroutine(LevelUpFlash());
    }

    private void Refresh()
    {
        if (AccountManager.Instance == null)
            return;

        int level = AccountManager.Instance.GetCurrentLevel();
        int currentExperience =
            AccountManager.Instance.GetCurrentExperience();

        int requiredExperience =
            AccountManager.Instance.GetExperienceRequiredForNextLevel();

        float progress =
            AccountManager.Instance.GetLevelProgressPercent();

        if (levelText != null)
            levelText.text = level.ToString();

        if (experienceProgressSlider != null)
        {
            experienceProgressSlider.minValue = 0f;
            experienceProgressSlider.maxValue = 1f;
            experienceProgressSlider.value = progress;
        }

        if (sliderFillImage != null)
            sliderFillImage.color = normalFillColor;

        bool isMaxLevel = level >= 20;

        if (experienceText != null)
        {
            experienceText.text = isMaxLevel
                ? string.Empty
                : currentExperience.ToString();
        }

        if (requiredExperienceText != null)
        {
            requiredExperienceText.text = isMaxLevel
                ? "MAX"
                : $"/ {requiredExperience} XP";
        }

        Debug.Log(
            $"[BackpackStoryLevelDisplay] " +
            $"Level: {level}, XP: {currentExperience}/{requiredExperience}");
    }

    private IEnumerator LevelUpFlash()
    {
        if (sliderFillImage == null)
            yield break;

        sliderFillImage.color = levelUpFillColor;

        yield return new WaitForSecondsRealtime(0.5f);

        sliderFillImage.color = normalFillColor;
        levelUpCoroutine = null;
    }
}