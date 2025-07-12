using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class SaveLoadManagerController : MonoBehaviour
{
    [Header("References")]
    public DialogueManager dialogueManager;
    public HeroStats heroStats;
    public ItemManager itemManager;

    // Reflection handles for private members
    private FieldInfo dialogueIndexField;
    private MethodInfo showLineMethod;
    private FieldInfo collectedItemNamesField;

    private void Awake()
    {
        // Assign references if not set in inspector
        if (dialogueManager == null)
            dialogueManager = FindObjectOfType<DialogueManager>();
        if (heroStats == null && HeroStats.Instance != null)
            heroStats = HeroStats.Instance;
        if (itemManager == null && ItemManager.Instance != null)
            itemManager = ItemManager.Instance;

        // Prepare reflection to access private fields/methods
        dialogueIndexField = typeof(DialogueManager)
            .GetField("currentLineIndex", BindingFlags.NonPublic | BindingFlags.Instance);
        showLineMethod = typeof(DialogueManager)
            .GetMethod("ShowLine", BindingFlags.NonPublic | BindingFlags.Instance);
        collectedItemNamesField = typeof(ItemManager)
            .GetField("collectedItemNames", BindingFlags.NonPublic | BindingFlags.Instance);

        if (dialogueIndexField == null)
            Debug.LogError("SaveLoadManager: Cannot find 'currentLineIndex' on DialogueManager.");
        if (showLineMethod == null)
            Debug.LogError("SaveLoadManager: Cannot find 'ShowLine' on DialogueManager.");
        if (collectedItemNamesField == null)
            Debug.LogError("SaveLoadManager: Cannot find 'collectedItemNames' on ItemManager.");
    }

    /// <summary>
    /// Saves current dialogue index, hero stats, and collected item names to PlayerPrefs.
    /// </summary>
    public void SaveGame()
    {
        // Save dialogue position
        if (dialogueManager != null && dialogueIndexField != null)
        {
            int currentIndex = (int)dialogueIndexField.GetValue(dialogueManager);
            PlayerPrefs.SetInt("Save_DialogueIndex", currentIndex);
            Debug.Log($"SaveLoadManager: Dialogue index saved = {currentIndex}");
        }
        else
        {
            Debug.LogWarning("SaveLoadManager: DialogueManager reference or field missing.");
        }

        // Save hero stats
        if (heroStats != null)
        {
            PlayerPrefs.SetInt("Save_Bloodthirst", heroStats.bloodthirstPoints);
            PlayerPrefs.SetInt("Save_Noble", heroStats.noblePoints);
            PlayerPrefs.SetInt("Save_Love", heroStats.lovePoints);
            Debug.Log($"SaveLoadManager: HeroStats saved - Bloodthirst={heroStats.bloodthirstPoints}, Noble={heroStats.noblePoints}, Love={heroStats.lovePoints}");
        }
        else
        {
            Debug.LogWarning("SaveLoadManager: HeroStats reference missing.");
        }

        // Save collected items
        if (itemManager != null)
        {
            int count = itemManager.collectedItems.Count;
            PlayerPrefs.SetInt("Save_ItemCount", count);
            for (int i = 0; i < count; i++)
            {
                string itemName = itemManager.collectedItems[i].itemName;
                PlayerPrefs.SetString($"Save_Item_{i}", itemName);
            }
            Debug.Log($"SaveLoadManager: Saved {count} collected items.");
        }
        else
        {
            Debug.LogWarning("SaveLoadManager: ItemManager reference missing.");
        }

        PlayerPrefs.Save();
        Debug.Log("SaveLoadManager: All data saved to PlayerPrefs.");
    }

    /// <summary>
    /// Loads saved dialogue index, hero stats, and collected items from PlayerPrefs.
    /// </summary>
    public void LoadGame()
    {
        // Load dialogue position
        if (dialogueManager != null && dialogueIndexField != null)
        {
            if (PlayerPrefs.HasKey("Save_DialogueIndex"))
            {
                int loadedIndex = PlayerPrefs.GetInt("Save_DialogueIndex");
                dialogueIndexField.SetValue(dialogueManager, loadedIndex);
                Debug.Log($"SaveLoadManager: Dialogue index loaded = {loadedIndex}");
                // Refresh display
                showLineMethod?.Invoke(dialogueManager, null);
            }
            else
            {
                Debug.Log("SaveLoadManager: No saved dialogue index found.");
            }
        }

        // Load hero stats
        if (heroStats != null)
        {
            if (PlayerPrefs.HasKey("Save_Bloodthirst"))
            {
                heroStats.bloodthirstPoints = PlayerPrefs.GetInt("Save_Bloodthirst");
                heroStats.noblePoints = PlayerPrefs.GetInt("Save_Noble");
                heroStats.lovePoints = PlayerPrefs.GetInt("Save_Love");
                Debug.Log($"SaveLoadManager: HeroStats loaded - Bloodthirst={heroStats.bloodthirstPoints}, Noble={heroStats.noblePoints}, Love={heroStats.lovePoints}");
            }
            else
            {
                Debug.Log("SaveLoadManager: No saved hero stats found.");
            }
        }

        // Load collected items
        if (itemManager != null)
        {
            // Clear existing stored data
            itemManager.collectedItems.Clear();
            if (collectedItemNamesField != null)
            {
                var nameSet = collectedItemNamesField.GetValue(itemManager) as HashSet<string>;
                nameSet?.Clear();
            }

            if (PlayerPrefs.HasKey("Save_ItemCount"))
            {
                int count = PlayerPrefs.GetInt("Save_ItemCount");
                for (int i = 0; i < count; i++)
                {
                    string key = $"Save_Item_{i}";
                    if (PlayerPrefs.HasKey(key))
                    {
                        string itemName = PlayerPrefs.GetString(key);
                        // Create minimal ItemNotification with only name
                        ItemNotification newItem = new ItemNotification { itemName = itemName };
                        itemManager.AddItem(newItem);
                    }
                }
                Debug.Log($"SaveLoadManager: Loaded {PlayerPrefs.GetInt("Save_ItemCount")} items.");
            }
            else
            {
                Debug.Log("SaveLoadManager: No saved items found.");
            }
        }

        Debug.Log("SaveLoadManager: Load complete.");
    }
}