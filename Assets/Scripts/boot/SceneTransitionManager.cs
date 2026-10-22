using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    [Serializable]
    private class LoadingTip
    {
        [TextArea] public string russianTitle;
        [TextArea] public string russianDescription;
        [TextArea] public string englishTitle;
        [TextArea] public string englishDescription;
    }

    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition")]
    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField] private CanvasGroup loadingIndicator;
    [SerializeField] private RectTransform loadingArc;

    [Header("Loading Tips")]
    [SerializeField] private CanvasGroup tipsGroup;
    [SerializeField] private TMP_Text tipTitle;
    [SerializeField] private TMP_Text tipDescription;
    [SerializeField] private LoadingTip[] tips;
    [SerializeField] private float tipDuration = 8f;

    [Header("Loading")]
    [SerializeField] private float minimumLoadingTime = 4f;
    [SerializeField] private float maximumLoadingTime = 10f;
    [SerializeField] private float weakDeviceMaximumLoadingTime = 22f;
    [SerializeField] private float rotationDuration = 1.2f;

    [Header("Fade")]
    [SerializeField] private float loadingIndicatorFadeDuration = 0.3f;
    [SerializeField] private float blackOverlayFadeDuration = 1f;
    [SerializeField] private float preLoadingFadeDuration = 0.35f;

    private Coroutine spinnerCoroutine;
    private Coroutine tipsCoroutine;
    private bool isLoading;
    private int lastTipIndex = -1;
    private readonly HashSet<int> shownTipIndices = new HashSet<int>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ЭТОТ объект переживает смену сцены.
        DontDestroyOnLoad(gameObject);

        // Экран изначально не блокируем.
        blackOverlay.alpha = 0f;
        blackOverlay.gameObject.SetActive(false);

        loadingIndicator.alpha = 0f;
        loadingIndicator.gameObject.SetActive(false);

        if (tipsGroup != null)
        {
            tipsGroup.alpha = 0f;
            tipsGroup.gameObject.SetActive(false);
        }

        LanguageManager.OnLanguageChanged += UpdateCurrentTip;
    }

    private void OnDestroy()
    {
        LanguageManager.OnLanguageChanged -= UpdateCurrentTip;
    }


    public void LoadScene(
        string sceneName,
        Action onSceneLoaded = null,
        bool showTips = true,
        bool fadeBeforeLoading = false)
    {
        if (isLoading)
            return;

        StartCoroutine(
            LoadSceneRoutine(sceneName, onSceneLoaded, showTips, fadeBeforeLoading)
        );
    }


    private IEnumerator LoadSceneRoutine(
        string sceneName,
        Action onSceneLoaded,
        bool showTips,
        bool fadeBeforeLoading)
    {
        isLoading = true;

        float startTime =
            Time.unscaledTime;

        float targetLoadingTime = GetTargetLoadingTime();

        if (fadeBeforeLoading)
        {
            blackOverlay.gameObject.SetActive(true);
            blackOverlay.alpha = 0f;
            yield return StartCoroutine(
                FadeCanvasGroup(blackOverlay, 1f, preLoadingFadeDuration));
        }

        // =====================================================
        // 1. МГНОВЕННО ЗАКРЫВАЕМ ТЕКУЩУЮ СЦЕНУ
        // =====================================================

        blackOverlay.gameObject.SetActive(true);
        blackOverlay.alpha = 1f;


        // =====================================================
        // 2. ПОКАЗЫВАЕМ SPINNER
        // =====================================================

        loadingIndicator.gameObject.SetActive(true);
        loadingIndicator.alpha = 1f;

        spinnerCoroutine =
            StartCoroutine(
                AnimateSpinner()
            );

        if (showTips)
        {
            tipsCoroutine = StartCoroutine(ShowTips());
        }


        // =====================================================
        // 3. ЗАГРУЖАЕМ НОВУЮ СЦЕНУ
        // =====================================================

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                sceneName
            );

        // Не активируем её автоматически.
        operation.allowSceneActivation = false;


        // Ждём полной подготовки сцены.
        while (operation.progress < 0.9f)
        {
            yield return null;
        }


        // =====================================================
        // 4. Минимальная и адаптивная длительность загрузочного экрана.
        // =====================================================

        float elapsed =
            Time.unscaledTime - startTime;

        if (elapsed < targetLoadingTime)
        {
            yield return new WaitForSecondsRealtime(
                targetLoadingTime - elapsed
            );
        }


        // =====================================================
        // 5. АКТИВИРУЕМ MAIN MENU
        // =====================================================

        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            yield return null;
        }


        // =====================================================
        // 6. MAIN MENU УЖЕ АКТИВНА,
        //    НО ЧЁРНЫЙ ЭКРАН ОСТАЁТСЯ
        // =====================================================

        // Даём MainMenu один кадр.
        yield return null;

        onSceneLoaded?.Invoke();


        // =====================================================
        // 7. УБИРАЕМ SPINNER
        // =====================================================

        if (spinnerCoroutine != null)
        {
            StopCoroutine(
                spinnerCoroutine
            );

            spinnerCoroutine = null;
        }

        if (tipsCoroutine != null)
        {
            StopCoroutine(tipsCoroutine);
            tipsCoroutine = null;
        }

        HideTips();

        yield return StartCoroutine(
            FadeCanvasGroup(
                loadingIndicator,
                0f,
                loadingIndicatorFadeDuration
            )
        );


        // =====================================================
        // 8. ОСВЕЩАЕМ MAIN MENU
        // =====================================================

        yield return StartCoroutine(
            FadeCanvasGroup(
                blackOverlay,
                0f,
                blackOverlayFadeDuration
            )
        );


        // =====================================================
        // 9. Менеджер остаётся доступным для следующих переходов.
        // =====================================================

        isLoading = false;
    }

    private float GetTargetLoadingTime()
    {
        int score = GetDevicePerformanceScore();
        float upperBound;

        switch (score)
        {
            case 0:
                upperBound = weakDeviceMaximumLoadingTime;
                break;
            case 1:
                upperBound = Mathf.Lerp(
                    maximumLoadingTime,
                    weakDeviceMaximumLoadingTime,
                    0.65f);
                break;
            case 2:
                upperBound = maximumLoadingTime;
                break;
            default:
                upperBound = Mathf.Lerp(
                    minimumLoadingTime,
                    maximumLoadingTime,
                    0.5f);
                break;
        }

        return UnityEngine.Random.Range(
            minimumLoadingTime,
            Mathf.Max(minimumLoadingTime, upperBound));
    }

    private int GetDevicePerformanceScore()
    {
        int score = 0;

        if (SystemInfo.processorCount >= 8)
            score++;

        if (SystemInfo.systemMemorySize >= 8192)
            score++;

        if (SystemInfo.graphicsMemorySize >= 4096)
            score++;

        return score;
    }

    private IEnumerator ShowTips()
    {
        if (tipsGroup == null || tipTitle == null ||
            tipDescription == null || tips == null || tips.Length == 0)
            yield break;

        tipsGroup.gameObject.SetActive(true);
        tipsGroup.alpha = 1f;
        ShowNextTip();

        while (true)
        {
            yield return new WaitForSecondsRealtime(tipDuration);
            ShowNextTip();
        }
    }

    private void ShowNextTip()
    {
        if (tips == null || tips.Length == 0 || tipTitle == null ||
            tipDescription == null)
            return;

        int nextIndex = GetRandomTipIndex();
        LoadingTip tip = tips[nextIndex];
        bool isEnglish = LanguageManager.CurrentLanguage == Language.English;

        tipTitle.text = isEnglish ? tip.englishTitle : tip.russianTitle;
        tipDescription.text = isEnglish
            ? tip.englishDescription
            : tip.russianDescription;

        lastTipIndex = nextIndex;
    }

    private int GetRandomTipIndex()
    {
        if (tips.Length == 1)
            return 0;

        if (shownTipIndices.Count >= tips.Length)
            shownTipIndices.Clear();

        int nextIndex;
        do
        {
            nextIndex = UnityEngine.Random.Range(0, tips.Length);
        }
        while (shownTipIndices.Contains(nextIndex) ||
               (tips.Length > 1 && nextIndex == lastTipIndex));

        shownTipIndices.Add(nextIndex);

        return nextIndex;
    }

    private void UpdateCurrentTip(Language language)
    {
        if (isLoading && tipsGroup != null && tipsGroup.gameObject.activeSelf)
            ShowCurrentTip();
    }

    private void ShowCurrentTip()
    {
        if (lastTipIndex < 0 || lastTipIndex >= tips.Length)
            return;

        LoadingTip tip = tips[lastTipIndex];
        bool isEnglish = LanguageManager.CurrentLanguage == Language.English;
        tipTitle.text = isEnglish ? tip.englishTitle : tip.russianTitle;
        tipDescription.text = isEnglish
            ? tip.englishDescription
            : tip.russianDescription;
    }

    private void HideTips()
    {
        if (tipsGroup == null)
            return;

        tipsGroup.alpha = 0f;
        tipsGroup.gameObject.SetActive(false);
    }


    private IEnumerator AnimateSpinner()
    {
        if (loadingArc == null)
        {
            Debug.LogError(
                "SceneTransitionManager: loadingArc is not assigned."
            );

            yield break;
        }

        float rotationSpeed =
            360f / rotationDuration;

        while (true)
        {
            loadingArc.Rotate(
                0f,
                0f,
                -rotationSpeed *
                Time.unscaledDeltaTime,
                Space.Self
            );

            yield return null;
        }
    }


    private IEnumerator FadeCanvasGroup(
        CanvasGroup group,
        float targetAlpha,
        float duration)
    {
        if (group == null)
            yield break;

        float startAlpha =
            group.alpha;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    elapsed / duration
                );

            group.alpha =
                Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    progress
                );

            yield return null;
        }

        group.alpha = targetAlpha;

        if (targetAlpha <= 0f)
        {
            group.gameObject.SetActive(false);
        }
    }
}