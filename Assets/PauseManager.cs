using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    [SerializeField] private BackpackItemsPanel backpack;

    [Header("Root")]
    [SerializeField] public CanvasGroup pauseRoot;

    [Header("Panels")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup savePanel;

    [Header("Exit Confirm")]
    [SerializeField] private CanvasGroup exitConfirmPanel;
    [SerializeField] private float confirmFadeDuration = 0.2f;

    private bool isExitConfirmActive;


    [Header("Animation")]
    [SerializeField] private float pauseFadeDuration = 0.3f;
    [SerializeField] private float panelFadeDuration = 0.15f;

    public bool IsPaused => isPaused;
    private bool isPaused;
    private bool isAnimating;

    private PauseNotification pauseNotification;

    #region Unity

    private void Awake()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Не создавать PauseManager в главном меню
        if (currentScene == "MainMenu")
        {
            Destroy(gameObject);
            return;
        }

        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        pauseNotification = pauseRoot.GetComponentInChildren<PauseNotification>(true);
        HideInstant();
    }

    private void Update()
    {
        if (pauseRoot == null)
            return;

        if (!Input.GetKeyDown(KeyCode.Escape) &&
            !Input.GetKeyDown(KeyCode.Space))
            return;

        // Рюкзак сам обрабатывает Escape.
        if (BackpackSectionController.Instance != null &&
            BackpackSectionController.Instance.IsBackpackOpen() && Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        // Подтверждение выхода.
        if (isExitConfirmActive)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ConfirmExit();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                HideExitConfirm();
            }

            return;
        }

        // Вложенные панели.
        if (IsPanelActive(settingsPanel))
        {
            SwitchPanel(settingsPanel, mainPanel);
            return;
        }

        if (IsPanelActive(savePanel))
        {
            SwitchPanel(savePanel, mainPanel);
            return;
        }

        // Пауза.
        if (isPaused)
        {
            Resume();
            return;
        }

        if (BackpackSectionController.Instance != null &&
    BackpackSectionController.Instance.JustClosedBackpack)
        {
            return;
        }

        Pause();
    }


    #endregion

    #region Core Pause

    public void Pause()
    {
        if (isPaused || isAnimating) return;

        isPaused = true;
        Time.timeScale = 0f;

        pauseRoot.gameObject.SetActive(true);
        pauseRoot.alpha = 0f;
        pauseRoot.blocksRaycasts = true;
        pauseRoot.interactable = true;

        DisableAllPanels();

        mainPanel.gameObject.SetActive(true);
        mainPanel.alpha = 1f;
        mainPanel.interactable = true;
        mainPanel.blocksRaycasts = true;


        StartCoroutine(Fade(pauseRoot, 0f, 1f, pauseFadeDuration));

        if (pauseNotification != null)
            pauseNotification.ShowPauseText();
    }

    public void Resume()
    {
        if (!isPaused || isAnimating) return;

        if (pauseNotification != null)
            pauseNotification.HidePauseText();

        StartCoroutine(ResumeRoutine());
    }

    private IEnumerator ResumeRoutine()
    {
        isAnimating = true;

        yield return Fade(pauseRoot, 1f, 0f, pauseFadeDuration);

        HideInstant();
        Time.timeScale = 1f;

        isPaused = false;
        isAnimating = false;
    }

    #endregion

    #region Panels

    public void OpenSettings()
    {
        SwitchPanel(mainPanel, settingsPanel);
    }

    public void OpenSave()
    {
        SwitchPanel(mainPanel, savePanel);
    }

    private void SwitchPanel(CanvasGroup from, CanvasGroup to)
    {
        if (from == to || isAnimating) return;

        StartCoroutine(SwitchPanelRoutine(from, to));
    }

    private IEnumerator SwitchPanelRoutine(CanvasGroup from, CanvasGroup to)
    {
        isAnimating = true;

        // Fade out old
        yield return Fade(from, 1f, 0f, panelFadeDuration);
        from.interactable = false;
        from.blocksRaycasts = false;
        from.gameObject.SetActive(false);

        // Fade in new
        to.gameObject.SetActive(true);
        to.alpha = 0f;
        to.interactable = true;
        to.blocksRaycasts = true;

        yield return Fade(to, 0f, 1f, panelFadeDuration);

        isAnimating = false;
    }


    #endregion

    #region Fade

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        float t = 0f;
        group.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        group.alpha = to;
    }

    #endregion

    #region Utils

    private bool IsPanelActive(CanvasGroup panel)
    {
        return panel.gameObject.activeSelf && panel.alpha > 0.9f;
    }

    private void DisableAllPanels()
    {
        DisablePanel(mainPanel);
        DisablePanel(settingsPanel);
        DisablePanel(savePanel);
    }

    private void DisablePanel(CanvasGroup panel)
    {
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    public void HideInstant()
    {
        DisableAllPanels();

        pauseRoot.alpha = 0f;
        pauseRoot.blocksRaycasts = false;
        pauseRoot.interactable = false;
        pauseRoot.gameObject.SetActive(false);

        isPaused = false;
        isAnimating = false;
    }

    #endregion

    #region UI Buttons

    public void OnContinuePressed()
    {
        Resume();
    }

    public void OnExitPressed()
    {
        if (isExitConfirmActive) return;
        ShowExitConfirm();
    }

    private void ShowExitConfirm()
    {
        isExitConfirmActive = true;

        // блокируем основную панель
        mainPanel.interactable = false;
        mainPanel.blocksRaycasts = false;

        exitConfirmPanel.interactable = true;
        exitConfirmPanel.blocksRaycasts = true;

        StartCoroutine(Fade(exitConfirmPanel, 0f, 1f, confirmFadeDuration));
    }

    private void HideExitConfirm()
    {
        isExitConfirmActive = false;

        exitConfirmPanel.interactable = false;
        exitConfirmPanel.blocksRaycasts = false;

        StartCoroutine(Fade(exitConfirmPanel, 1f, 0f, confirmFadeDuration));

        mainPanel.interactable = true;
        mainPanel.blocksRaycasts = true;
    }

    private void ConfirmExit()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("woollybunny_PC");
    }

    #endregion
}
