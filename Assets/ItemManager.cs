using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    private HashSet<string> collectedItemNames = new HashSet<string>();
    public List<ItemNotification> collectedItems = new List<ItemNotification>();

    [SerializeField] private ItemAddedUI itemUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // сохраняется между сценами
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(ItemNotification item)
    {
        if (!collectedItemNames.Contains(item.itemName))
        {
            collectedItemNames.Add(item.itemName);
            collectedItems.Add(item);

            if (StatisticsManager.Instance != null)
                StatisticsManager.Instance.AddCollectedItem();

            Debug.Log("Предмет добавлен: " + item.itemName);

            if (itemUI != null)
            {
                itemUI.ShowNotificationDirect(item);
            }

            // Здесь можно сделать запись в сохранение
        }
        else
        {
            Debug.Log("Этот предмет уже был найден: " + item.itemName);
        }
    }

    public bool HasItem(string itemName)
    {
        return collectedItemNames.Contains(itemName);
    }

    public void ShowItemNotification(ItemNotification line)
    {
        ItemNotification newItem = new ItemNotification
        {
            itemName = line.itemName,
            itemType = line.itemType,
            rarity = line.rarity,
            itemIcon = line.itemIcon
        };

        if (itemUI != null)
            itemUI.ShowNotificationDirect(newItem);
        else
            Debug.LogWarning("Item UI is not assigned!");
    }
}

