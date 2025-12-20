using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChapterButton : MonoBehaviour
{
    [Tooltip("Название сцены для этой главы")]
    public string sceneName;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Scene name is empty on " + name);
        }
    }
}
