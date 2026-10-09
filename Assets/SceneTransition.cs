using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public string sceneToLoad = "Chapter";

    public void OnNewGameClicked()
    {
        if (SceneTransitionManager.Instance != null)
        {
            SceneTransitionManager.Instance.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.LogError("SceneTransitionManager не найден.");
        }
    }
}