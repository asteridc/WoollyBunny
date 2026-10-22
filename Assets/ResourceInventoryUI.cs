using UnityEngine;

public class ResourceInventoryUI : MonoBehaviour
{
    [SerializeField] private Transform uniqueItemsContainer;
    [SerializeField] private ResourceItemView uniqueItemPrefab;

    public Transform UniqueItemsContainer => uniqueItemsContainer;
    public ResourceItemView UniqueItemPrefab => uniqueItemPrefab;

    private void Start()
    {
        if (ResourceInventoryManager.Instance != null)
            ResourceInventoryManager.Instance.RegisterInventoryUI(this);
    }
}
