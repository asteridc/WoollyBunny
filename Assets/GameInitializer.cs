using UnityEngine;

public class GameInitializer : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("[GameInitializer] Инициализация игры начата");

        // Убеждаемся, что SaveManager существует
        if (SaveManager.Instance == null)
        {
            Debug.LogWarning("[GameInitializer] SaveManager.Instance был null, создаем...");
            SaveManager.GetOrCreate();
        }
        Debug.Log("[GameInitializer] SaveManager: OK");

        // Убеждаемся, что AccountManager существует
        if (AccountManager.Instance == null)
        {
            Debug.LogWarning("[GameInitializer] AccountManager.Instance был null, создаем...");
            AccountManager.GetOrCreate();
        }
        Debug.Log("[GameInitializer] AccountManager: OK");

        Debug.Log("[GameInitializer] Инициализация завершена");
    }
}
