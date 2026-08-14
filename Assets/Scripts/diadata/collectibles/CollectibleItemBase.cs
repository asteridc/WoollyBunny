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
    [Header("Collection")]
    [SerializeField] private string id;
    [SerializeField] private CollectibleCategory category;

    public CollectibleType type;

    public string Id => id;
    public CollectibleCategory Category => category;
    public CollectibleType Type => type;

    public virtual Sprite GetIcon()
    {
        return null;
    }

    public virtual string GetTitle()
    {
        return name;
    }

    public virtual string GetContent()
    {
        return string.Empty;
    }
}