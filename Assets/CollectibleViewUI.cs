using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CollectibleViewUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI itemTitleText;
    [SerializeField] private TextMeshProUGUI itemContentText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private GameObject panel;

    private CollectibleItemBase currentItem;

    private void OnEnable()
    {
        LanguageManager.OnLanguageChanged += Refresh;
    }

    private void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= Refresh;
    }

    public void Show(CollectibleItemBase item)
    {
        if (item == null) return;

        currentItem = item;

        switch (item.type)
        {
            case CollectibleType.Note:
            case CollectibleType.Document:
                ShowTextItem(item as CollectibleTextItem);
                break;

            default:
                Debug.LogWarning($"Collectible type {item.type} not supported yet");
                break;
        }

        panel.SetActive(true);
    }

    public void Hide()
    {
        currentItem = null;
        panel.SetActive(false);
    }

    private void Refresh(Language _)
    {
        if (currentItem == null) return;
        Show(currentItem);
    }

    private void ShowTextItem(CollectibleTextItem item)
    {
        if (item == null) return;

        itemTitleText.text = item.GetTitle();
        itemContentText.text = item.GetContent();
        itemIcon.sprite = item.icon;
    }
}
