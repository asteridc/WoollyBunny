using DG.Tweening;
using System.Collections;
using System.IO;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]

    private const int CONTINUE_SLOT = 5;

    [Header("Panels")]
    [SerializeField] private CanvasGroup mainPanel;
    [SerializeField] private CanvasGroup savePanel;
    [SerializeField] private CanvasGroup settingsPanel;
    [SerializeField] private CanvasGroup chapterSelectPanel;

    [Header("Animation")]
    [SerializeField] private float panelFadeDuration = 0.75f;

    [SerializeField] private RectTransform chapterContainer;
    [SerializeField] private float scrollDuration = 0.4f;
    [SerializeField] private float cardSpacing = 600f; // рассто€ние между картами

    private int currentIndex = 0;
    private int totalChapters;

    private CanvasGroup activePanel;
    private bool isAnimating;

    private void Start()
    {
        // √лавное меню всегда активное
        mainPanel.alpha = 1f;
        mainPanel.interactable = true;
        mainPanel.blocksRaycasts = true;

        totalChapters = chapterContainer.childCount;

        // Overlay-панели изначально скрыты
        HidePanelInstant(settingsPanel);
        HidePanelInstant(chapterSelectPanel);

        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (activePanel != null && !isAnimating)
            {
                // «акрываем overlay-панель
                StartCoroutine(ClosePanel(activePanel));
                activePanel = null;
            }
        }
    }

    public void ScrollLeft()
    {
        if (currentIndex <= 0) return;
        currentIndex--;
        ScrollToCurrent();
    }

    public void ScrollRight()
    {
        if (currentIndex >= totalChapters - 1) return;
        currentIndex++;
        ScrollToCurrent();
    }

    private void ScrollToCurrent()
    {
        Vector2 targetPos = new Vector2(-currentIndex * cardSpacing, 0f);
        chapterContainer.DOAnchorPos(targetPos, scrollDuration).SetEase(Ease.OutCubic);
    }

    private string GetSavePath(int slot)
    {
        return Application.persistentDataPath + $"/save_{slot}.json";
    }

    #region Button Actions

    public void OnContinueClicked()
    {
        SaveManager.Instance.LoadGame(CONTINUE_SLOT);
    }

    public void OnNewGameClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Chapter_01");
    }

    public void OnSaveClicked()
    {
        OpenPanel(savePanel);
    }

    public void OnBackFromSave()
    {
        StartCoroutine (ClosePanel(savePanel));
        activePanel = null;
    }

    public void OnSettingsClicked()
    {
        OpenPanel(settingsPanel);
    }

    public void OnChapterSelectClicked()
    {
        OpenPanel(chapterSelectPanel);
    }

    public void OnBackFromSettings()
    {
        StartCoroutine(ClosePanel(settingsPanel));
        activePanel = null;
    }

    public void OnBackFromChapterSelect()
    {
        StartCoroutine(ClosePanel(chapterSelectPanel));
        activePanel = null;
    }

    public void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    #region Panel Control

    private void OpenPanel(CanvasGroup panel)
    {
        if (isAnimating || panel == null) return;

        activePanel = panel;
        panel.alpha = 0f;
        panel.gameObject.SetActive(true);
        panel.interactable = true;
        panel.blocksRaycasts = true;

        StartCoroutine(Fade(panel, 0f, 1f, panelFadeDuration));
    }

    public IEnumerator ClosePanel(CanvasGroup panel)
    {
        if (panel == null || isAnimating) yield break;

        isAnimating = true;

        yield return Fade(panel, panel.alpha, 0f, panelFadeDuration);

        HidePanelInstant(panel);

        isAnimating = false;
    }

    private void HidePanelInstant(CanvasGroup panel)
    {
        if (panel == null) return;

        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        panel.gameObject.SetActive(false);
    }

    #endregion

    #region Fade

    private IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
    {
        isAnimating = true;

        float t = 0f;
        group.alpha = from;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }

        group.alpha = to;
        isAnimating = false;
    }

    #endregion
}
