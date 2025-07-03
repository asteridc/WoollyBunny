using UnityEngine;
using UnityEngine.SceneManagement;

public class NewGameLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Chapter_01"; // имя сцены первой главы

    public void StartNewGame()
    {
        PlayerPrefs.DeleteAll(); // сброс прогресса
        SceneManager.LoadScene(firstSceneName);
    }
}