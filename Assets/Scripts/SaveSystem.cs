using UnityEngine;

public static class SaveSystem
{
    private const string KEY = "cardgame_save_v1";

    public static void Save(GameState state)
    {
        string json = JsonUtility.ToJson(state);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Saved state");
    }

    public static GameState Load()
    {
        if (!PlayerPrefs.HasKey(KEY)) return null;
        string json = PlayerPrefs.GetString(KEY);
        try
        {
            return JsonUtility.FromJson<GameState>(json);
        }
        catch
        {
            Debug.LogWarning("Failed to parse saved state");
            return null;
        }
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(KEY);
    }
}
