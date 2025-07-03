using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject hiddenPanel;
    public GameObject shownPanel;

    public void OpenChapterSelect()
    {
        Debug.Log("Кнопка нажата!");
        hiddenPanel.SetActive(false);
        shownPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        shownPanel.SetActive(false);
        hiddenPanel.SetActive(true);
    }
}