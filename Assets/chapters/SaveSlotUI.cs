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

    private void OnEnable()
    {
        StartCoroutine(DelayedRefresh());
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

        chapterText.text = $"Глава {data.dialogueData.chapterIndex + 1}";
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
