using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance { get; private set; }

    [Header("Transition")]
    [SerializeField] private CanvasGroup blackOverlay;
    [SerializeField] private CanvasGroup loadingIndicator;
    [SerializeField] private RectTransform loadingArc;

    [Header("Loading")]
    [SerializeField] private float minimumLoadingTime = 4f;
    [SerializeField] private float rotationDuration = 1.2f;

    [Header("Fade")]
    [SerializeField] private float loadingIndicatorFadeDuration = 0.3f;
    [SerializeField] private float blackOverlayFadeDuration = 1f;

    private Coroutine spinnerCoroutine;
    private bool isLoading;


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
    }


    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(
            LoadSceneRoutine(sceneName)
        );
    }


    private IEnumerator LoadSceneRoutine(
        string sceneName)
    {
        isLoading = true;

        float startTime =
            Time.unscaledTime;


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
        // 4. МИНИМУМ 4 СЕКУНДЫ
        // =====================================================

        float elapsed =
            Time.unscaledTime - startTime;

        if (elapsed < minimumLoadingTime)
        {
            yield return new WaitForSecondsRealtime(
                minimumLoadingTime - elapsed
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
        // 9. TRANSITION MANAGER БОЛЬШЕ НЕ НУЖЕН
        // =====================================================

        isLoading = false;

        Destroy(gameObject);
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