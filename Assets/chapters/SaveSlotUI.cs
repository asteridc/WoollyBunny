using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections;

public class SaveSlotUI : MonoBehaviour
{
    public int slotIndex;

    [Header("UI")]
    public Image screenshotImage;
    public TextMeshProUGUI chapterText;
    public TextMeshProUGUI timeText;

    private string SavePath =>
        Application.persistentDataPath + $"/save_{slotIndex}.json";

    private void Awake()
    {
        DisableChapterTextLocalization();
    }

    private void OnEnable()
    {
        DisableChapterTextLocalization();
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
        StartCoroutine(DelayedRefresh());
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
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

        chapterText.text = GetSaveTitle(data.dialogueData);
        timeText.text = data.saveTime;

        screenshotImage.color = Color.gray; // временный фон
    }

    private void OnLanguageChanged(Language language)
    {
        StartCoroutine(DelayedRefresh());
    }

    private void DisableChapterTextLocalization()
    {
        if (chapterText == null)
            return;

        LocalizedText localizedText = chapterText.GetComponent<LocalizedText>();
        if (localizedText != null && localizedText.enabled)
            localizedText.enabled = false;
    }

    private string GetSaveTitle(DialogueSaveData dialogueData)
    {
        if (dialogueData == null)
            return LanguageManager.CurrentLanguage == Language.English ? "New Game" : "Новая игра";

        int chapterNumber = GetChapterNumber(dialogueData);
        string prefix = LanguageManager.CurrentLanguage == Language.English ? "Chapter" : "Глава";

        return chapterNumber > 0
            ? $"{prefix} {chapterNumber}"
            : $"{prefix} ?";
    }

    private int GetChapterNumber(DialogueSaveData dialogueData)
    {
        if (dialogueData == null)
            return 0;

        if (dialogueData.chapterNumber > 0)
            return dialogueData.chapterNumber;

        return dialogueData.chapterIndex >= 0
            ? dialogueData.chapterIndex + 1
            : 0;
    }

    private void SetEmpty()
    {
        chapterText.text = LanguageManager.CurrentLanguage == Language.English
            ? "Empty Slot"
            : "Пустой слот";
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
            SaveManager.Instance.LoadGame(slotIndex);
        }
    }

    public void Delete()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        StartCoroutine(DelayedRefresh());
    }
}
