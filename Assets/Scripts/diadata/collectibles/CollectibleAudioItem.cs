using UnityEngine;

[CreateAssetMenu(
    fileName = "CollectibleAudioItem",
    menuName = "Localization/Collectible Item/Audio Item"
)]
public class CollectibleAudioItem : CollectibleItemBase
{
    private void OnEnable()
    {
        type = CollectibleType.Audio;
    }
}
