using System;

[Serializable]
public sealed class HalloweenPromoCodeRecord
{
    public string code;
    public string offerId;
    public string rewardInventoryId;

    public int pricePaid;
    public int basketCapacityPercent;

    public long createdAtUnixSeconds;
    public long expiresAtUnixSeconds;

    public bool isRedeemed;
    public long redeemedAtUnixSeconds;

    public HalloweenPromoCodeRecord Copy()
    {
        return new HalloweenPromoCodeRecord
        {
            code = code,
            offerId = offerId,
            rewardInventoryId = rewardInventoryId,

            pricePaid = pricePaid,
            basketCapacityPercent = basketCapacityPercent,

            createdAtUnixSeconds = createdAtUnixSeconds,
            expiresAtUnixSeconds = expiresAtUnixSeconds,

            isRedeemed = isRedeemed,
            redeemedAtUnixSeconds = redeemedAtUnixSeconds
        };
    }
}