using UnityEngine;
using System.IO;
public static class SaveSystem
{
    private static string GetSavePath(int slot) =>
        Application.persistentDataPath + $"/save_{slot}.json";

    public static void SaveGame(int slot, GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slot), json);
    }

    public static GameSaveData LoadGame(int slot)
    {
        string path = GetSavePath(slot);
        if (!File.Exists(path)) return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<GameSaveData>(json);
    }
}
