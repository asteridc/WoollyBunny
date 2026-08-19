using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;
using UnityEngine.Rendering;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private const string ChapterProgressFileName = "chapter_progress.json";

    [Serializable]
    private class ChapterProgressData
    {
        public int highestUnlockedChapterNumber = 1;
    }

    private ChapterProgressData chapterProgressCache;

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

    private IEnumerator Start()
    {
        yield return null;

        SynchronizeCompletedChapterExperience();
    }

    public static SaveManager GetOrCreate()
    {
        if (Instance != null)
            return Instance;

        Debug.LogWarning("[SaveManager] Instance был null, создаем новый GameObject...");
        GameObject go = new GameObject("SaveManager");
        return go.AddComponent<SaveManager>();
    }


    public bool IsChapterUnlocked(int chapterNumber)
    {
        if (chapterNumber <= 1)
            return true;

        return LoadChapterProgress().highestUnlockedChapterNumber >= chapterNumber;
    }

    public void MarkChapterCompleted(DialogueChapter chapter)
    {
        if (chapter == null)
            return;

        MarkChapterCompleted(chapter.ChapterNumber);
    }

    public void MarkChapterCompleted(int chapterNumber)
    {
        Debug.Log(
            $"[SaveManager] MarkChapterCompleted вызвана для главы {chapterNumber}");

        if (chapterNumber < 1)
            return;

        ChapterProgressData data = LoadChapterProgress();

        int nextUnlockedChapter = Mathf.Max(
            data.highestUnlockedChapterNumber,
            chapterNumber + 1);

        bool progressChanged =
            nextUnlockedChapter > data.highestUnlockedChapterNumber;

        if (progressChanged)
        {
            data.highestUnlockedChapterNumber = nextUnlockedChapter;
            SaveChapterProgress(data);

            Debug.Log(
                $"[SaveManager] Глава {chapterNumber} отмечена завершённой. " +
                $"Открыта глава {nextUnlockedChapter}.");
        }
        else
        {
            Debug.Log(
                $"[SaveManager] Глава {chapterNumber} уже была завершена.");
        }

        // Награду проверяем всегда, даже если глава была завершена раньше.
        AccountManager accountManager = AccountManager.GetOrCreate();

        if (accountManager == null)
        {
            Debug.LogError(
                "[SaveManager] Не удалось получить AccountManager.");

            return;
        }

        bool rewardGranted =
            accountManager.TryGrantChapterExperience(chapterNumber);

        if (rewardGranted)
        {
            Debug.Log(
                $"[SaveManager] Выдан опыт за главу {chapterNumber}.");
        }
    }

    public void SynchronizeCompletedChapterExperience()
    {
        ChapterProgressData progressData = LoadChapterProgress();

        int highestUnlockedChapter =
            Mathf.Max(1, progressData.highestUnlockedChapterNumber);

        int highestCompletedChapter =
            highestUnlockedChapter - 1;

        if (highestCompletedChapter < 1)
        {
            Debug.Log(
                "[SaveManager] Нет завершённых глав для синхронизации опыта.");

            return;
        }

        AccountManager accountManager = AccountManager.GetOrCreate();

        if (accountManager == null)
        {
            Debug.LogError(
                "[SaveManager] Не удалось получить AccountManager " +
                "для синхронизации наград.");

            return;
        }

        int rewardedCount = 0;

        for (int chapterNumber = 1;
             chapterNumber <= highestCompletedChapter;
             chapterNumber++)
        {
            if (accountManager.TryGrantChapterExperience(chapterNumber))
                rewardedCount++;
        }

        Debug.Log(
            $"[SaveManager] Синхронизация наград завершена. " +
            $"Новых наград: {rewardedCount}.");
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
        Debug.Log(Application.persistentDataPath);

        //if (StatisticsManager.Instance != null)
        //    StatisticsManager.Instance.AddGameLaunch();

        var data = new GameSaveData
        {
            sceneName = "Chapter_01",
            dialogueData = null, // диалог начнётся с начала
            saveTime = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            //statistics =
            //    StatisticsManager.Instance != null
            //        ? StatisticsManager.Instance.GetStatistics()
            //        : new StatisticsData
            //        {
            //            firstLaunchDate = System.DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
            //            gameLaunches = 1
            //        },

            electroMinigameActive = false,
            guitarMinigameActive = false,
            loops = new System.Collections.Generic.List<LoopSaveState>(),

        };

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetPath(slot), json);
    }


    //private StatisticsData LoadStatistics(int slot)
    //{
    //    string path = GetPath(slot);

    //    if (!File.Exists(path))
    //        return new StatisticsData();

    //    string json = File.ReadAllText(path);

    //    GameSaveData oldData = JsonUtility.FromJson<GameSaveData>(json);

    //    if (oldData.statistics == null)
    //        oldData.statistics = new StatisticsData();

    //    return oldData.statistics;
    //}

    public void SaveGame(int slot)
    {
        StartCoroutine(CaptureScreenshot(slot));

        var dm = DialogueManager.Instance;

        var data = new GameSaveData
        {
            sceneName = SceneManager.GetActiveScene().name,
            dialogueData = dm.CaptureDialogueState(),
            saveTime = System.DateTime.Now.ToString("MM.dd.yyyy HH:mm"),

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

        // Уровень Сюжета загружается из AccountData, а не из слота
        // Он уже загружен в AccountManager при инициализации

        // 🔹 Скрываем паузу до скрина
        if (PauseManager.Instance != null)
            PauseManager.Instance.HideInstant();

        Time.timeScale = 1f;

        // 🔹 Новая сцена
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
            // 🔹 Та же сцена — явно сбросить UI и восстановить диалог
            if (PauseManager.Instance != null)
                PauseManager.Instance.HideInstant();
            if (data.dialogueData != null)
                StartCoroutine(RestoreDialogueWhenReady(data.dialogueData));
        }

        // 🔹 Скриншот после скрытия UI
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
        // 1️⃣ Загрузка сцены
        if (SceneManager.GetActiveScene().name != data.sceneName)
        {
            SceneManager.LoadScene(data.sceneName);
            yield return null;
            yield return null;
        }

        // 2️⃣ Ждём DialogueManager
        while (DialogueManager.Instance == null)
            yield return null;

        // 3️⃣ СБРОС МИРА ДО ВОССТАНОВЛЕНИЯ
        ResetWorld();
        ResetLoops();

        // 4️⃣ ВОССТАНОВЛЕНИЕ ДИАЛОГА (КЛЮЧЕВОЕ МЕСТО)
        StartCoroutine(RestoreDialogueWhenReady(data.dialogueData));

        // 5️⃣ Восстановление лупов ПОСЛЕ диалога
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



    private string GetPath(int slot)
    {
        return Application.persistentDataPath + $"/save_{slot}.json";
    }
    private string GetChapterProgressPath()
    {
        return Path.Combine(Application.persistentDataPath, ChapterProgressFileName);
    }

    private ChapterProgressData LoadChapterProgress()
    {
        if (chapterProgressCache != null)
            return chapterProgressCache;

        string path = GetChapterProgressPath();

        if (!File.Exists(path))
        {
            chapterProgressCache = new ChapterProgressData();
            return chapterProgressCache;
        }

        try
        {
            string json = File.ReadAllText(path);
            chapterProgressCache = JsonUtility.FromJson<ChapterProgressData>(json) ?? new ChapterProgressData();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SAVE MANAGER] Failed to read chapter progress, using defaults. {ex.Message}");
            chapterProgressCache = new ChapterProgressData();
        }

        if (chapterProgressCache.highestUnlockedChapterNumber < 1)
            chapterProgressCache.highestUnlockedChapterNumber = 1;

        return chapterProgressCache;
    }

    private void SaveChapterProgress(ChapterProgressData data)
    {
        if (data == null)
            return;

        chapterProgressCache = data;
        File.WriteAllText(GetChapterProgressPath(), JsonUtility.ToJson(data, true));
    }

    public void CompleteChapterAndPrepareNext(int chapterNumber)
    {
        Debug.Log(
            $"[SaveManager] Завершаем главу {chapterNumber} и подготавливаем переход в следующую.");

        // 1. Фиксируем завершение главы.
        // Здесь же автоматически выдается XP.
        MarkChapterCompleted(chapterNumber);

        // 2. AccountManager уже хранит постоянный прогресс отдельно
        // от обычного сохранения слота.
        AccountManager accountManager = AccountManager.GetOrCreate();

        if (accountManager == null)
        {
            Debug.LogError(
                "[SaveManager] Не удалось получить AccountManager при переходе между главами.");
            return;
        }

        Debug.Log(
            $"[SaveManager] Постоянный прогресс подготовлен для перехода после главы {chapterNumber}.");
    }

    [Serializable]
    private class ChapterTransitionData
    {
        public int completedChapter;
        public int bloodthirst;
        public int nobility;
        public int love;
    }

    private const string ChapterTransitionFileName = "chapter_transition.json";

    public void SaveChapterTransition(int completedChapter)
    {
        if (completedChapter < 1)
        {
            Debug.LogWarning("[SaveManager] Некорректный номер главы.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError(
                "[SaveManager] DialogueManager.Instance == null.");
            return;
        }

        // Сначала фиксируем завершение главы.
        // Здесь же выдаётся XP и открывается следующая глава.
        MarkChapterCompleted(completedChapter);

        // Получаем текущие очки путей прямо из DialogueManager.
        var pathPoints = DialogueManager.Instance.GetPathPoints();

        ChapterTransitionData data = new ChapterTransitionData
        {
            completedChapter = completedChapter,
            bloodthirst = pathPoints.bloodthirst,
            nobility = pathPoints.nobility,
            love = pathPoints.love
        };

        string path = GetChapterTransitionPath();
        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);

        Debug.Log(
            $"[SaveManager] Прогресс перехода сохранён: " +
            $"Chapter={completedChapter}, " +
            $"Bloodthirst={data.bloodthirst}, " +
            $"Nobility={data.nobility}, " +
            $"Love={data.love}");
    }

    public void LoadChapterTransition(int expectedChapter)
    {
        string path = GetChapterTransitionPath();

        if (!File.Exists(path))
        {
            Debug.Log("[SaveManager] Сохранение перехода между главами не найдено.");
            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogError(
                "[SaveManager] DialogueManager.Instance == null.");
            return;
        }

        try
        {
            string json = File.ReadAllText(path);

            ChapterTransitionData data =
                JsonUtility.FromJson<ChapterTransitionData>(json);

            if (data == null)
            {
                Debug.LogWarning(
                    "[SaveManager] Не удалось прочитать данные перехода.");
                return;
            }

            if (data.completedChapter + 1 != expectedChapter)
            {
                Debug.LogWarning(
                    $"[SaveManager] Данные перехода относятся не к этой главе. " +
                    $"Saved={data.completedChapter}, Expected={expectedChapter}");
                return;
            }

            DialogueManager.Instance.SetPathPoints(
                data.bloodthirst,
                data.nobility,
                data.love
            );

            Debug.Log(
                $"[SaveManager] Очки путей восстановлены: " +
                $"Bloodthirst={data.bloodthirst}, " +
                $"Nobility={data.nobility}, " +
                $"Love={data.love}");
        }
        catch (Exception ex)
        {
            Debug.LogError(
                $"[SaveManager] Ошибка загрузки перехода между главами: {ex.Message}");
        }
    }

    private string GetChapterTransitionPath()
    {
        return Path.Combine(
            Application.persistentDataPath,
            "chapter_transition.json");
    }

}




