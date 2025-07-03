using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    public static PlayerLevel Instance; // Синглтон для удобного доступа

    public int currentExperience = 0; // Текущий опыт игрока
    public int maxExperience = 500;    // Максимальный опыт для следующего уровня

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Сохраняем между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }
}