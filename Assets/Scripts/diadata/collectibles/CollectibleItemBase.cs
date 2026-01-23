using UnityEngine;

public enum CollectibleType
{
    Note,
    Document,
    Audio,
    Artifact
}

public abstract class CollectibleItemBase : ScriptableObject
{
    public CollectibleType type;
}