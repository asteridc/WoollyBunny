using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    public static CollectibleManager Instance { get; private set; }

    private readonly List<CollectibleItemBase> collectedCollectibles = new();
    private readonly HashSet<string> collectedIds = new();

    public IReadOnlyList<CollectibleItemBase> CollectedCollectibles =>
        collectedCollectibles;

    public event Action<CollectibleItemBase> CollectibleAdded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool RegisterCollectible(
        CollectibleItemBase collectible)
    {
        if (collectible == null)
        {
            Debug.LogWarning(
                "CollectibleManager: collectible == null.",
                this
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(collectible.Id))
        {
            Debug.LogWarning(
                $"CollectibleManager: у '{collectible.name}' отсутствует ID.",
                collectible
            );

            return false;
        }

        // Предмет уже получен.
        if (!collectedIds.Add(collectible.Id))
        {
            Debug.Log(
                $"Коллекционный предмет уже найден: {collectible.Id}"
            );

            return false;
        }

        // Порядок получения сохраняем.
        collectedCollectibles.Add(collectible);

        Debug.Log(
            $"Коллекционный предмет добавлен: " +
            $"{collectible.Id} / {collectible.Category}"
        );

        CollectibleAdded?.Invoke(collectible);

        return true;
    }

    public bool HasCollectible(
        CollectibleItemBase collectible)
    {
        if (collectible == null)
            return false;

        return HasCollectible(collectible.Id);
    }

    public bool HasCollectible(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return false;

        return collectedIds.Contains(id);
    }

    /// <summary>
    /// Возвращает все полученные предметы категории
    /// в порядке их получения.
    /// </summary>
    public List<CollectibleItemBase> GetCollectedByCategory(
        CollectibleCategory category)
    {
        List<CollectibleItemBase> result = new();

        for (int i = 0; i < collectedCollectibles.Count; i++)
        {
            CollectibleItemBase collectible =
                collectedCollectibles[i];

            if (collectible != null &&
                collectible.Category == category)
            {
                result.Add(collectible);
            }
        }

        return result;
    }

    /// <summary>
    /// Возвращает фактическое количество полученных
    /// предметов в категории.
    /// </summary>
    public int GetCollectedCount(
        CollectibleCategory category)
    {
        int count = 0;

        for (int i = 0; i < collectedCollectibles.Count; i++)
        {
            CollectibleItemBase collectible =
                collectedCollectibles[i];

            if (collectible != null &&
                collectible.Category == category)
            {
                count++;
            }
        }

        return count;
    }

    public void ClearCollection()
    {
        collectedCollectibles.Clear();
        collectedIds.Clear();

        CollectibleAdded?.Invoke(null);

        Debug.Log("Коллекция полностью очищена.");
    }
}