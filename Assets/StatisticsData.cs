using System;
using UnityEngine;
[Serializable]  
public class StatisticsData
{
    // Игровой процесс
    public double totalPlayTime;
    public string firstLaunchDate;
    public int gameLaunches;

    // Прохождение
    public int completedSideMissions;
    public int foundCaches;
    public int choicesMade;
}