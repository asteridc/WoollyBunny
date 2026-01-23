using UnityEngine;

[CreateAssetMenu(
    fileName = "CollectibleArtifactItem",
    menuName = "Localization/Collectible Item/Artifact"
)]
public class CollectibleArtifactItem : CollectibleItemBase
{
    private void OnEnable()
    {
        type = CollectibleType.Artifact;
    }
}
