using UnityEngine;

public class CollectibleCategoryView : MonoBehaviour
{
    [Header("Category")]
    [SerializeField] private CollectibleCategory category;

    [Header("Slots")]
    [SerializeField] private Transform slotsContainer;

    private CollectibleSlotUI[] slots;

    private void Awake()
    {
        CacheSlots();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void CacheSlots()
    {
        if (slotsContainer == null)
            slotsContainer = transform;

        slots = slotsContainer
            .GetComponentsInChildren<CollectibleSlotUI>(true);
    }

    private void Subscribe()
    {
        if (CollectibleManager.Instance == null)
            return;

        CollectibleManager.Instance.CollectibleAdded -=
            OnCollectibleAdded;

        CollectibleManager.Instance.CollectibleAdded +=
            OnCollectibleAdded;
    }

    private void Unsubscribe()
    {
        if (CollectibleManager.Instance == null)
            return;

        CollectibleManager.Instance.CollectibleAdded -=
            OnCollectibleAdded;
    }

    private void OnCollectibleAdded(
        CollectibleItemBase collectible)
    {
        if (collectible == null)
            return;

        if (collectible.Category != category)
            return;

        Refresh();
    }

    public void Refresh()
    {
        if (CollectibleManager.Instance == null)
            return;

        if (slots == null || slots.Length == 0)
            CacheSlots();

        // Сначала всё возвращаем в состояние "не получено".
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].Clear();
        }

        var collected =
            CollectibleManager.Instance
                .GetCollectedByCategory(category);

        // Открываем только реально полученные предметы.
        for (int i = 0; i < collected.Count; i++)
        {
            CollectibleItemBase collectible = collected[i];

            for (int j = 0; j < slots.Length; j++)
            {
                if (slots[j].Matches(collectible))
                {
                    slots[j].SetCollected(true);
                    break;
                }
            }
        }
    }
}