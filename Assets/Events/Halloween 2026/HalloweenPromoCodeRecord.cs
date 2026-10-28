using System;

[Serializable]
public sealed class HalloweenPromoCodeRecord
{
    public string code;
    public string offerId;
    public string rewardInventoryId;

    // Quantity granted on redemption. Older saved UniqueItem codes may have 0;
    // redemption treats those legacy records as a quantity of 1.
    public int rewardAmount = 1;

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
            rewardAmount = rewardAmount,
            pricePaid = pricePaid,
            basketCapacityPercent = basketCapacityPercent,
            createdAtUnixSeconds = createdAtUnixSeconds,
            expiresAtUnixSeconds = expiresAtUnixSeconds,
            isRedeemed = isRedeemed,
            redeemedAtUnixSeconds = redeemedAtUnixSeconds
        };
    }
}
