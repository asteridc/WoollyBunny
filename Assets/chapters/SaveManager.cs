using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using UnityEngine.Rendering;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Debug.Log("[SAVE MANAGER] Awake");
    }


    private IEnumerator CaptureScreenshot(int slot)
    {
        // 1. Скрываем UI паузы и меню сохранения
        bool wasPauseVisible = PauseManager.Instance.IsPaused;
        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
        {
            PauseManager.Instance.HideInstant();
        }


        // 2. Ждём один кадр, чтобы всё обновилось
        yield return null;
        yield return new WaitForEndOfFrame();

        // 3. Берём скриншот
        int width = Screen.width;
        int height = Screen.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        string path = Application.persistentDataPath + $"/save_{slot}_screenshot.png";
        File.WriteAllBytes(path, bytes);

        Debug.Log($"[SAVE MANAGER] Screenshot saved to {path}");

        if (wasPauseVisible)
            PauseManager.Instance.Pause();
            PauseManager.Instance.OpenSave();
    }

    public void CreateNewGameSave(int slot)
    {
        Debug.Log($"[SAVE MANAGER] CreateNewGameSave slot {slot}");

        var data = new GameSaveData
        {
            sceneName = "Chapter_01",
            dialogueData = null, // диалог начнётся с начала
            saveTime = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"),

            saveLanguage = LanguageManager.CurrentLanguage.ToString(),

            electroMinigameActive = false,
            guitarMinigameActive = false,
            loops = new System.Collections.Generic.List<LoopSaveState>()
        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }


    public void SaveGame(int slot)
    {
        StartCoroutine(CaptureScreenshot(slot));

        var dm = DialogueManager.Instance;

        var data = new GameSaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            dialogueData = dm.CaptureDialogueState(),
            saveTime = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"),

            saveLanguage = LanguageManager.CurrentLanguage.ToString(),

            electroMinigameActive =
                ElectroChainManager.Instance != null &&
                ElectroChainManager.Instance.gameObject.activeSelf,

            guitarMinigameActive =
                dm.guitarInteractiveUI != null &&
                dm.guitarInteractiveUI.activeSelf,
        };

        var registry = LoopRegistry.Instance;

        if (registry != null)
        {
            foreach (var pair in registry.GetLoops())
            {
                data.loops.Add(new LoopSaveState
                {
                    id = pair.Key,
                    active = pair.Value.activeSelf
                });
            }
        }


        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }


    public void LoadGame(int slot)
    {
        string path = GetPath(slot);
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);

        ApplySavedLanguage(data);

        // ?? Скрываем паузу до скрина
        if (PauseManager.Instance != null)
            PauseManager.Instance.HideInstant();

        Time.timeScale = 1f;

        // ?? Новая сцена
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != data.sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) =>
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= null;
                if (data.dialogueData != null)
                    StartCoroutine(RestoreDialogueWhenReady(data.dialogueData));
            };
            UnityEngine.SceneManagement.SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            // ?? Та же сцена — явно сбросить UI и восстановить диалог
            if (PauseManager.Instance != null)
                PauseManager.Instance.HideInstant();
            if (data.dialogueData != null)
                StartCoroutine(RestoreDialogueWhenReady(data.dialogueData));
        }

        // ?? Скриншот после скрытия UI
        StartCoroutine(CaptureScreenshotAfterFrame(slot));
    }

    private IEnumerator CaptureScreenshotAfterFrame(int slot)
    {
        yield return new WaitForEndOfFrame();
        string path = Application.persistentDataPath + $"/screenshot_{slot}.png";
        ScreenCapture.CaptureScreenshot(path);
    }

    private IEnumerator LoadRoutine(GameSaveData data)
    {
        // 1?? Загрузка сцены
        if (SceneManager.GetActiveScene().name != data.sceneName)
        {
            SceneManager.LoadScene(data.sceneName);
            yield return null;
            yield return null;
        }

        // 2?? Ждём DialogueManager
        while (DialogueManager.Instance == null)
            yield return null;

        // 3?? СБРОС МИРА ДО ВОССТАНОВЛЕНИЯ
        ResetWorld();
        ResetLoops();

        // 4?? ВОССТАНОВЛЕНИЕ ДИАЛОГА (КЛЮЧЕВОЕ МЕСТО)
        StartCoroutine(RestoreDialogueWhenReady(data.dialogueData));

        // 5?? Восстановление лупов ПОСЛЕ диалога
        var registry = LoopRegistry.Instance;
        if (registry != null)
        {
            var dict = registry.GetLoops();

            foreach (var loop in data.loops)
            {
                if (dict.TryGetValue(loop.id, out var obj))
                    obj.SetActive(loop.active);
            }
        }
    }






    private IEnumerator RestoreAfterLoad(GameSaveData data)
    {
        // Ждём один кадр, чтобы сцена прогрузилась
        yield return null;

        // Восстанавливаем диалог
        DialogueManager.Instance.RestoreDialogueState(data.dialogueData);

        // Если есть UI для диалога — включаем
        DialogueManager.Instance.ShowDialoguePanel();
        DialogueManager.Instance.ShowLine();
    }


    private void ResetLoops()
    {
        var registry = LoopRegistry.Instance;
        if (registry == null) return;

        foreach (var pair in registry.GetLoops())
            pair.Value.SetActive(false);
    }

    private void ResetWorld()
    {
        // мини-игры
        if (ElectroChainManager.Instance != null)
            ElectroChainManager.Instance.ClosePanelAndContinue();

        var dm = DialogueManager.Instance;

        if (dm != null)
        {
            if (dm.guitarInteractiveUI != null)
                dm.guitarInteractiveUI.SetActive(false);

            if (dm.miniGamePanel != null)
                dm.miniGamePanel.SetActive(false);

            if (dm.interactionPointGroup != null)
                dm.interactionPointGroup.SetActive(false);
        }
    }

    private IEnumerator RestoreDialogueWhenReady(DialogueSaveData data)
    {
        while (DialogueManager.Instance == null || !DialogueManager.IsReady)
            yield return null;

        DialogueManager.Instance.RestoreDialogueState(data);
    }

    private void ApplySavedLanguage(GameSaveData data)
    {
        if (data == null || string.IsNullOrEmpty(data.saveLanguage))
            return;

        if (Enum.TryParse(data.saveLanguage, true, out Language saveLanguage))
            LanguageManager.SetLanguage(saveLanguage);
    }

    private string GetPath(int slot)
    {
        return Application.persistentDataPath + $"/save_{slot}.json";
    }
}




