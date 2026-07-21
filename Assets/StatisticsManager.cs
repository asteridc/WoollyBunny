using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;
    private StatisticsData statistics = new StatisticsData();
    private bool gameLaunchRegisteredThisSession;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureStatistics();
    }

    private void Update()
    {
        if (statistics == null)
            return;

        string sceneName = SceneManager.GetActiveScene().name;
        if (!sceneName.StartsWith("Chapter", StringComparison.OrdinalIgnoreCase))
            return;

        statistics.totalPlayTime += Time.deltaTime / 3600d;
    }

    public void AddGameLaunch()
    {
        EnsureStatistics();

        if (gameLaunchRegisteredThisSession)
            return;

        statistics.gameLaunches++;
        gameLaunchRegisteredThisSession = true;
    }

    public void AddChoice()
    {
        EnsureStatistics();
        statistics.choicesMade++;
    }

    public void SetStoryLevel(int level)
    {
        EnsureStatistics();
        statistics.storyLevel = Mathf.Max(statistics.storyLevel, Mathf.Max(0, level));
    }

    public void AddCompletedChapter()
    {
        EnsureStatistics();
        statistics.completedChapters = Mathf.Min(12, statistics.completedChapters + 1);
    }

    public void AddCompletedSideMission()
    {
        EnsureStatistics();
        statistics.completedSideMissions++;
    }

    public void AddFoundCache()
    {
        EnsureStatistics();
        statistics.foundCaches++;
    }

    public void AddCollectedDrawing()
    {
        EnsureStatistics();
        statistics.drawingsReceived++;
    }

    public void AddCollectedRecord()
    {
        EnsureStatistics();
        statistics.recordsReceived++;
    }

    public void AddCollectedItem()
    {
        EnsureStatistics();
        statistics.collectedItems++;
    }

    public void AddCollectedWeapon()
    {
        EnsureStatistics();
        statistics.weaponsReceived++;
    }

    public void AddCollectedResource()
    {
        EnsureStatistics();
        statistics.resourcesReceived++;
    }

    public void AddPathPoints(DominantPath path, int amount)
    {
        EnsureStatistics();

        if (amount <= 0)
            return;

        switch (path)
        {
            case DominantPath.Bloodthirst:
                statistics.bloodthirstPoints += amount;
                break;
            case DominantPath.Nobility:
                statistics.nobilityPoints += amount;
                break;
            case DominantPath.Love:
                statistics.lovePoints += amount;
                break;
        }
    }

    public void SetStatistics(StatisticsData data)
    {
        statistics = data ?? new StatisticsData();
        EnsureStatistics();
    }

    public StatisticsData GetStatistics()
    {
        EnsureStatistics();
        return statistics;
    }

    private void EnsureStatistics()
    {
        if (statistics == null)
            statistics = new StatisticsData();

        if (string.IsNullOrWhiteSpace(statistics.firstLaunchDate))
            statistics.firstLaunchDate = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
    }
}
