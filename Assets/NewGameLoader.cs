using UnityEngine;

public class NewGameLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Chapter_01"; // имя сцены первой главы

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll(); // сброс прогресса

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(firstSceneName);
        else
            Debug.LogError("SceneTransitionManager не найден.");
    }
}