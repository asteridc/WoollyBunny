using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class SceneTransition : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public string sceneToLoad = "Chapter";

    public void OnNewGameClicked()
    {
        fadePanel.gameObject.SetActive(true); // включение панели

        // затемнение за 0.5 секунды
        fadePanel.DOFade(1f, 2f).OnComplete(() =>
        {
            DOVirtual.DelayedCall(2f, () =>
            {
               SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            });
        });
    }
}