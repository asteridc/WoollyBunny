using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpecialStorySceneButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(LoadSpecialStory);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(LoadSpecialStory);
    }

    public void LoadSpecialStory()
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"{nameof(SpecialStorySceneButton)}: имя сцены не задано на объекте {name}.", this);
            return;
        }

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError($"{nameof(SpecialStorySceneButton)}: SceneTransitionManager не найден.", this);
            return;
        }

        button.interactable = false;
        SceneTransitionManager.Instance.LoadScene(
            sceneName,
            null,
            true,
            true);
    }
}
