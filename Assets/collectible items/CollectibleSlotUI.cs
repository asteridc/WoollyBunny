using UnityEngine;

public class CollectibleSlotUI : MonoBehaviour
{
    [Header("Collectible")]
    [SerializeField] private CollectibleItemBase collectible;

    [Header("Objects")]
    [SerializeField] private GameObject emptyObject;
    [SerializeField] private GameObject itemObject;

    public CollectibleItemBase Collectible => collectible;

    public bool IsOccupied { get; private set; }

    private void Awake()
    {
        ApplyState(false);
    }

    public bool Matches(CollectibleItemBase target)
    {
        return target != null &&
               collectible != null &&
               collectible.Id == target.Id;
    }

    public void SetCollected(bool collected)
    {
        IsOccupied = collected;
        ApplyState(collected);
    }

    private void ApplyState(bool collected)
    {
        if (emptyObject != null)
            emptyObject.SetActive(!collected);

        if (itemObject != null)
            itemObject.SetActive(collected);
    }

    public void Clear()
    {
        IsOccupied = false;
        ApplyState(false);
    }
}