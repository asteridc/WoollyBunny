using System.Collections.Generic;
using UnityEngine;

public class LoopRegistry : MonoBehaviour
{
    public static LoopRegistry Instance;

    [System.Serializable]
    public class LoopEntry
    {
        public string id;
        public GameObject loopObject;
    }

    public List<LoopEntry> loops = new();

    private void Awake()
    {
        Instance = this;
    }

    public Dictionary<string, GameObject> GetLoops()
    {
        var dict = new Dictionary<string, GameObject>();
        foreach (var l in loops)
            dict[l.id] = l.loopObject;
        return dict;
    }
}
