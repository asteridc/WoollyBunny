using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.IO;
using System.Collections;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;

    [Header("UI")]
    public Image screenshotImage;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI timeText;

    [Header("Localization Warning")]
    [SerializeField] private CanvasGroup localizationMismatchPanel;
    [SerializeField] private TextMeshProUGUI localizationMismatchText;
    [SerializeField] private float mismatchFadeDuration = 0.2f;

    private Coroutine mismatchFadeCoroutine;
    private bool isMismatchWarningActive;
    private Language pendingSaveLanguage;

    private string SavePath =>
        Application.persistentDataPath + $"/save_{slotIndex}.json";

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
        if (localizationMismatchPanel != null)
        {
            localizationMismatchPanel.alpha = 0f;
            localizationMismatchPanel.interactable = false;
            localizationMismatchPanel.blocksRaycasts = false;
            localizationMismatchPanel.gameObject.SetActive(false);
        }
        StartCoroutine(DelayedRefresh());
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    private void Update()
    {
        if (!isMismatchWarningActive)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ConfirmLocalizationMismatchLoad();
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            HideLocalizationMismatchWarning();
        }
    }

    // Обновление с задержкой, чтобы UI успел прогрузиться
    private IEnumerator DelayedRefresh()
    {
        yield return null; // ждем один кадр
        Refresh();
    }

    public void Refresh()
    {
        if (!File.Exists(SavePath))
        {
            SetEmpty();
            return;
        }

        string screenshotPath = Application.persistentDataPath + $"/save_{slotIndex}_screenshot.png";
        if (File.Exists(screenshotPath))
        {
            byte[] bytes = File.ReadAllBytes(screenshotPath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            screenshotImage.sprite = sprite;
        }
        else
        {
            screenshotImage.color = new Color(1, 1, 1, 0.1f); // пустой слот
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        if (data.dialogueData != null)
        {
            int chapterNumber = data.dialogueData.chapterIndex >= 0
                ? data.dialogueData.chapterIndex + 1
                : 1;
            string chapterPrefix = LanguageManager.CurrentLanguage == Language.English ? "Chapter" : "Глава";
            chapterText.text = $"{chapterPrefix} {chapterNumber}";
        }
        else
        {
            chapterText.text = "Новая игра";
        }
        timeText.text = data.saveTime;

        screenshotImage.color = Color.gray; // временный фон
    }

    private void SetEmpty()
    {
        chapterText.text = "Пустой слот";
        timeText.text = "";
        screenshotImage.color = new Color(1, 1, 1, 0f); // прозрачный
    }

    public void OnClick()
    {
        Debug.Log($"[SAVE SLOT] Click slot {slotIndex}");

        if (!File.Exists(SavePath))
        {
            SaveManager.Instance.SaveGame(slotIndex);
            StartCoroutine(DelayedRefresh());
        }
        else
        {
            string json = File.ReadAllText(SavePath);
            GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

            if (IsLocalizationMismatch(data, out Language saveLanguage))
            {
                pendingSaveLanguage = saveLanguage;
                ShowLocalizationMismatchWarning(saveLanguage);
                return;
            }

            SaveManager.Instance.LoadGame(slotIndex);
        }
    }

    public void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        StartCoroutine(DelayedRefresh());
    }

    public void HideLocalizationMismatchWarning()
    {
        if (localizationMismatchPanel == null)
            return;

        isMismatchWarningActive = false;
        localizationMismatchPanel.interactable = false;
        localizationMismatchPanel.blocksRaycasts = false;

        if (mismatchFadeCoroutine != null)
            StopCoroutine(mismatchFadeCoroutine);

        mismatchFadeCoroutine = StartCoroutine(FadeAndDisable(localizationMismatchPanel, localizationMismatchPanel.alpha, 0f, mismatchFadeDuration));
    }

    private void OnLanguageChanged(Language language)
    {
        Refresh();
    }

    private bool IsLocalizationMismatch(GameSaveData data, out Language saveLanguage)
    {
        saveLanguage = Language.Russian;
        if (data == null || string.IsNullOrEmpty(data.saveLanguage))
            return false;

        if (!Enum.TryParse(data.saveLanguage, true, out saveLanguage))
            return false;

        return saveLanguage != LanguageManager.CurrentLanguage;
    }

    private void ShowLocalizationMismatchWarning(Language saveLanguage)
    {
        if (localizationMismatchPanel == null)
            return;

        isMismatchWarningActive = true;

        localizationMismatchPanel.gameObject.SetActive(true);
        localizationMismatchPanel.interactable = true;
        localizationMismatchPanel.blocksRaycasts = true;

        if (mismatchFadeCoroutine != null)
            StopCoroutine(mismatchFadeCoroutine);

        mismatchFadeCoroutine = StartCoroutine(Fade(localizationMismatchPanel, 0f, 1f, mismatchFadeDuration));
    }

    private void ConfirmLocalizationMismatchLoad()
    {
        HideLocalizationMismatchWarning();

        LanguageManager.SetLanguage(pendingSaveLanguage);
        SaveManager.Instance.LoadGame(slotIndex);
    }

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

    private IEnumerator FadeAndDisable(CanvasGroup group, float from, float to, float duration)
    {
        yield return Fade(group, from, to, duration);

        if (Mathf.Approximately(to, 0f))
            group.gameObject.SetActive(false);
    }
}
