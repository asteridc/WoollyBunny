using UnityEngine;

[CreateAssetMenu(
    fileName = "HalloweenExchangeOffer",
    menuName = "Woolly Bunny/Halloween/Exchange Offer")]
public sealed class HalloweenExchangeOffer : ScriptableObject
{
    [Header("Offer")]
    [SerializeField] private string offerId;
    [SerializeField] private bool available = true;

    [Header("Reward")]
    [Tooltip("ID предмета из каталога ResourceInventoryManager. Поддерживаются UniqueItem и StackableResource.")]
    [SerializeField] private string rewardInventoryId;

    [Tooltip("Количество для StackableResource. Для UniqueItem всегда выдаётся 1.")]
    [SerializeField, Min(1)] private int rewardAmount = 1;

    [Header("Cost and basket")]
    [SerializeField, Min(1)] private int candyPrice = 1500;
    [SerializeField, Range(1, 100)] private int basketCapacityPercent = 100;

    public string OfferId => offerId;
    public bool Available => available;
    public string RewardInventoryId => rewardInventoryId;
    public int RewardAmount => Mathf.Max(1, rewardAmount);
    public int CandyPrice => candyPrice;
    public int BasketCapacityPercent => basketCapacityPercent;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!string.IsNullOrWhiteSpace(offerId))
            offerId = offerId.Trim();

        if (!string.IsNullOrWhiteSpace(rewardInventoryId))
            rewardInventoryId = rewardInventoryId.Trim();

        rewardAmount = Mathf.Max(1, rewardAmount);
        candyPrice = Mathf.Max(1, candyPrice);
        basketCapacityPercent = Mathf.Clamp(basketCapacityPercent, 1, 100);
    }
#endif
}
