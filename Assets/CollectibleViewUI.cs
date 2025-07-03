using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CollectibleViewUI : MonoBehaviour
{
    public TextMeshProUGUI itemTitleText;
    public TextMeshProUGUI itemContentText;
    public Image itemIcon;
    public GameObject panel;

    public void Show(string title, string content, Sprite icon)
    {
        itemTitleText.text = title;
        itemContentText.text = content;
        itemIcon.sprite = icon;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}