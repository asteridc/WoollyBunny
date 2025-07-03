using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class ItemAddedUI : MonoBehaviour
{
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemTypeText;
    public Image itemIcon;
    public GameObject panel;

    public List<ItemNotification> notifications; // <--- Вот они
    private int currentIndex = 0;

    public float displayTime = 3f;
    private float timer;
    private bool isShowing = false;

    void Start()
    {
        panel.SetActive(false);
    }

    void Update()
    {
        if (isShowing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                panel.SetActive(false);
                isShowing = false;
            }
        }
    }

    public void ShowNotification(int index)
    {
        if (index < 0 || index >= notifications.Count)
        {
            Debug.LogWarning("Index за пределами списка");
            return;
        }

        ItemNotification data = notifications[index];

        itemNameText.text = data.itemName;
        itemTypeText.text = data.itemType;
        itemTypeText.color = GetColorByRarity(data.rarity);
        itemIcon.sprite = data.itemIcon;

        panel.SetActive(true);
        timer = displayTime;
        isShowing = true;
    }

    public void ShowNotificationDirect(ItemNotification item)
    {
        itemNameText.text = item.itemName;
        itemTypeText.text = item.itemType;
        itemTypeText.color = GetColorByRarity(item.rarity);
        itemIcon.sprite = item.itemIcon;

        panel.SetActive(true);
        timer = displayTime;
        isShowing = true;
    }

    private Color GetColorByRarity(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common: return Color.gray;
            case ItemRarity.Rare: return new Color(0.2f, 0.6f, 1f);       // синий
            case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);     // фиолетовый
            case ItemRarity.Legendary: return new Color(1f, 0.6f, 0f);    // оранжевый
            default: return Color.white;
        }
    }
}

public enum ItemRarity
{
    Common,
    Rare,
    Epic,
    Legendary
}