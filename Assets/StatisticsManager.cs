using UnityEngine;

public class StatisticsManager : MonoBehaviour
{
    public static StatisticsManager Instance;
    private StatisticsData statistics;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (statistics == null)
            return;

        statistics.totalPlayTime += Time.deltaTime;
    }

    public void AddGameLaunch()
    {
        statistics.gameLaunches++;
    }

    public void AddChoice()
    {
        statistics.choicesMade++;
    }

    public void SetStatistics(StatisticsData data)
    {
        statistics = data;
    }

    public StatisticsData GetStatistics()
    {
        return statistics;
    }
}