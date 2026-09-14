using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BootController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private CanvasGroup loadingPanel;
    [SerializeField] private CanvasGroup disclaimerPanel;
    [SerializeField] private CanvasGroup agreementPanel;
    [SerializeField] private CanvasGroup logoPanel;

    [Header("Initial Loading Indicator")]
    [SerializeField] private RectTransform loadingArc;
    [SerializeField] private CanvasGroup loadingIndicatorGroup;
    [SerializeField] private float loadingIndicatorDelay = 1.5f;
    [SerializeField] private float loadingIndicatorFadeDuration = 0.3f;
    [SerializeField] private float loadingRotationDuration = 1.2f;

    [Header("Initial Loading")]
    [SerializeField] private float minimumLoadingTime = 5f;

    [Header("Logo")]
    [SerializeField] private CanvasGroup pressAnyKeyGroup;
    [SerializeField] private float pressAnyKeyFadeSpeed = 1f;

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "woollybunny_PC";

    [Header("Agreement")]
    [SerializeField] private string agreementPlayerPrefsKey = "AgreementAccepted";
    [SerializeField] private ScrollRect agreementScrollRect;
    [SerializeField] private CanvasGroup agreementControlsGroup;

    [SerializeField, Range(0f, 1f)]
    private float agreementLockedAlpha = 0.3f;

    [SerializeField]
    private float agreementUnlockedAlpha = 1f;

    [SerializeField]
    private float agreementScrollThreshold = 0.01f;

    [Header("UI Transitions")]
    [SerializeField] private float panelFadeDuration = 0.35f;

    private Coroutine loadingIndicatorCoroutine;
    private Coroutine pressAnyKeyCoroutine;


    private void Awake()
    {
        // НИЧЕГО не делаем через DontDestroyOnLoad.
        // BootController должен уничтожиться вместе с Boot-сценой.

        InitializeVisualState();
    }


    private void Start()
    {
        StartCoroutine(BootSequence());
    }


    private void InitializeVisualState()
    {
        SetCanvasGroup(loadingPanel, 1f);
        SetCanvasGroup(disclaimerPanel, 0f);
        SetCanvasGroup(agreementPanel, 0f);
        SetCanvasGroup(logoPanel, 0f);

        SetCanvasGroup(loadingIndicatorGroup, 0f);
        SetCanvasGroup(pressAnyKeyGroup, 0f);
    }


    private IEnumerator BootSequence()
    {
        // =====================================================
        // 1. INITIAL LOADING
        // =====================================================

        float loadingStartTime = Time.unscaledTime;

        loadingIndicatorCoroutine =
            StartCoroutine(AnimateLoadingIndicator());

        yield return StartCoroutine(LoadPlayerData());

        // Минимум 5 секунд начальной загрузки.
        float elapsedTime =
            Time.unscaledTime - loadingStartTime;

        if (elapsedTime < minimumLoadingTime)
        {
            yield return new WaitForSecondsRealtime(
                minimumLoadingTime - elapsedTime
            );
        }

        // Останавливаем spinner.
        if (loadingIndicatorCoroutine != null)
        {
            StopCoroutine(loadingIndicatorCoroutine);
            loadingIndicatorCoroutine = null;
        }

        yield return StartCoroutine(
            FadeOutCanvasGroup(
                loadingIndicatorGroup,
                panelFadeDuration
            )
        );

        yield return StartCoroutine(
            HidePanel(loadingPanel)
        );


        // =====================================================
        // 2. DISCLAIMER
        // =====================================================

        yield return StartCoroutine(
            ShowPanel(disclaimerPanel)
        );

        yield return StartCoroutine(
            WaitForDisclaimer()
        );

        yield return StartCoroutine(
            HidePanel(disclaimerPanel)
        );


        // =====================================================
        // 3. AGREEMENT
        // =====================================================

        bool agreementAccepted =
            PlayerPrefs.GetInt(
                agreementPlayerPrefsKey,
                0
            ) == 1;

        if (!agreementAccepted)
        {
            yield return StartCoroutine(
                ShowPanel(agreementPanel)
            );

            yield return StartCoroutine(
                WaitForAgreement()
            );

            yield return StartCoroutine(
                HidePanel(agreementPanel)
            );
        }


        // =====================================================
        // 4. LOGO
        // =====================================================

        yield return StartCoroutine(
            ShowPanel(logoPanel)
        );

        yield return StartCoroutine(
            WaitForAnyKey()
        );


        // =====================================================
        // 5. MAIN MENU TRANSITION
        // =====================================================

        // Очень важно:
        // сначала запускаем TransitionManager.
        // Он сразу накрывает экран чёрным.
        SceneTransitionManager.Instance.LoadScene(
            mainMenuSceneName
        );

        // После этого можем убрать Logo.
        // Пользователь уже этого не увидит,
        // потому что поверх него находится BlackOverlay.
        yield return StartCoroutine(
            HidePanel(logoPanel)
        );
    }


    // =========================================================
    // PLAYER DATA
    // =========================================================

    private IEnumerator LoadPlayerData()
    {
        // Пока заглушка.
        yield return null;

        // Здесь позже подключим существующую систему.
    }


    // =========================================================
    // DISCLAIMER
    // =========================================================

    private IEnumerator WaitForDisclaimer()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                yield break;
            }

            yield return null;
        }
    }


    // =========================================================
    // AGREEMENT
    // =========================================================

    private IEnumerator WaitForAgreement()
    {
        // Сначала элементы управления заблокированы.
        SetCanvasGroupAlpha(
            agreementControlsGroup,
            agreementLockedAlpha
        );

        // Ждём, пока пользователь доскроллит документ.
        while (!IsAgreementAtBottom())
        {
            yield return null;
        }

        // Документ прочитан.
        yield return StartCoroutine(
            FadeCanvasGroup(
                agreementControlsGroup,
                agreementUnlockedAlpha,
                0.3f
            )
        );

        // Теперь разрешаем принятие / отказ.
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Return) ||
                Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                AcceptAgreement();
                yield break;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                RejectAgreement();
                yield break;
            }

            yield return null;
        }
    }


    private bool IsAgreementAtBottom()
    {
        if (agreementScrollRect == null)
            return false;

        return agreementScrollRect.verticalNormalizedPosition
               <= agreementScrollThreshold;
    }


    private void AcceptAgreement()
    {
        PlayerPrefs.SetInt(
            agreementPlayerPrefsKey,
            1
        );

        PlayerPrefs.Save();
    }


    private void RejectAgreement()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    // =========================================================
    // LOGO
    // =========================================================

    private IEnumerator WaitForAnyKey()
    {
        pressAnyKeyCoroutine =
            StartCoroutine(
                AnimatePressAnyKey()
            );

        // Не даём предыдущему нажатию
        // автоматически закрыть Logo.
        yield return null;

        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        if (pressAnyKeyCoroutine != null)
        {
            StopCoroutine(pressAnyKeyCoroutine);
            pressAnyKeyCoroutine = null;
        }
    }


    private IEnumerator AnimatePressAnyKey()
    {
        pressAnyKeyGroup.gameObject.SetActive(true);
        pressAnyKeyGroup.alpha = 0.25f;

        while (true)
        {
            float time = 0f;

            while (time < 1f)
            {
                time +=
                    Time.unscaledDeltaTime *
                    pressAnyKeyFadeSpeed;

                float progress =
                    Mathf.Clamp01(time);

                pressAnyKeyGroup.alpha =
                    Mathf.Lerp(
                        0.25f,
                        1f,
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            progress
                        )
                    );

                yield return null;
            }

            time = 0f;

            while (time < 1f)
            {
                time +=
                    Time.unscaledDeltaTime *
                    pressAnyKeyFadeSpeed;

                float progress =
                    Mathf.Clamp01(time);

                pressAnyKeyGroup.alpha =
                    Mathf.Lerp(
                        1f,
                        0.25f,
                        Mathf.SmoothStep(
                            0f,
                            1f,
                            progress
                        )
                    );

                yield return null;
            }
        }
    }


    // =========================================================
    // INITIAL LOADING INDICATOR
    // =========================================================

    private IEnumerator AnimateLoadingIndicator()
    {
        yield return new WaitForSecondsRealtime(
            loadingIndicatorDelay
        );

        yield return StartCoroutine(
            FadeInCanvasGroup(
                loadingIndicatorGroup,
                loadingIndicatorFadeDuration
            )
        );

        if (loadingArc == null)
        {
            Debug.LogError(
                "BootController: loadingArc is not assigned."
            );

            yield break;
        }

        float rotationSpeed =
            720f / loadingRotationDuration;

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


    // =========================================================
    // UI TRANSITIONS
    // =========================================================

    private IEnumerator ShowPanel(CanvasGroup panel)
    {
        if (panel == null)
            yield break;

        panel.gameObject.SetActive(true);
        panel.alpha = 0f;

        float time = 0f;

        while (time < panelFadeDuration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    time / panelFadeDuration
                );

            panel.alpha =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            yield return null;
        }

        panel.alpha = 1f;
    }


    private IEnumerator HidePanel(CanvasGroup panel)
    {
        if (panel == null)
            yield break;

        float time = 0f;

        while (time < panelFadeDuration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    time / panelFadeDuration
                );

            panel.alpha =
                Mathf.SmoothStep(
                    1f,
                    0f,
                    progress
                );

            yield return null;
        }

        panel.alpha = 0f;
        panel.gameObject.SetActive(false);
    }


    private IEnumerator FadeInCanvasGroup(
        CanvasGroup group,
        float duration)
    {
        if (group == null)
            yield break;

        group.gameObject.SetActive(true);

        float startAlpha = group.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    time / duration
                );

            group.alpha =
                Mathf.Lerp(
                    startAlpha,
                    1f,
                    progress
                );

            yield return null;
        }

        group.alpha = 1f;
    }


    private IEnumerator FadeOutCanvasGroup(
        CanvasGroup group,
        float duration)
    {
        if (group == null)
            yield break;

        float startAlpha = group.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    time / duration
                );

            group.alpha =
                Mathf.Lerp(
                    startAlpha,
                    0f,
                    progress
                );

            yield return null;
        }

        group.alpha = 0f;
    }


    private IEnumerator FadeCanvasGroup(
        CanvasGroup group,
        float targetAlpha,
        float duration)
    {
        if (group == null)
            yield break;

        float startAlpha = group.alpha;
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float progress =
                Mathf.Clamp01(
                    time / duration
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
    }


    // =========================================================
    // HELPERS
    // =========================================================

    private void SetCanvasGroup(
        CanvasGroup group,
        float alpha)
    {
        if (group == null)
            return;

        group.alpha = alpha;
        group.gameObject.SetActive(alpha > 0f);
    }


    private void SetCanvasGroupAlpha(
        CanvasGroup group,
        float alpha)
    {
        if (group == null)
            return;

        group.alpha = alpha;
    }
}