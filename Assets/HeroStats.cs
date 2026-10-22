using UnityEngine;

public enum HeroPath
{
    None,
    Bloodthirst,
    Noble,
    Love
}

public class HeroStats : MonoBehaviour
{
    public static HeroStats Instance;

    public int bloodthirstPoints = 0;
    public int noblePoints = 0;
    public int lovePoints = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // сохраняем при смене сцен
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddPoints(HeroPath path, int amount)
    {
        switch (path)
        {
            case HeroPath.Bloodthirst:
                bloodthirstPoints += amount;
                break;
            case HeroPath.Noble:
                noblePoints += amount;
                break;
            case HeroPath.Love:
                lovePoints += amount;
                break;
        }

        if (amount > 0)
        {
            switch (path)
            {
                case HeroPath.Bloodthirst:
                    DailyQuestManager.Instance?.RegisterAction("BloodthirstPoints", amount);
                    break;
                case HeroPath.Noble:
                    DailyQuestManager.Instance?.RegisterAction("NobilityPoints", amount);
                    break;
                case HeroPath.Love:
                    DailyQuestManager.Instance?.RegisterAction("LovePoints", amount);
                    break;
            }
        }

        Debug.Log($"Добавлено {amount} очков в путь {path}");
    }
}
