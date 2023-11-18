using Internal.Core;
using UnityEngine;

public class SaveManagerBase<T> : Singleton<T> where T : MonoBehaviour
{
    [SerializeField] public SaveData saveData;
    [SerializeField] public bool DELETE_AT_START;

    public virtual void Awake()
    {
        if (DELETE_AT_START)
        {
            PlayerPrefs.DeleteAll();
        }
        Load();
    }

    public void Save()
    {
        var outputString = JsonUtility.ToJson(saveData);
        // Debug.Log(outputString);
        // Debug.LogWarning("Saved");
        if (!string.IsNullOrEmpty(outputString))
        {
            PlayerPrefs.SetString(nameof(SaveData), outputString);
        }
    }

    private void Load()
    {
        string inputString = PlayerPrefs.GetString(nameof(SaveData), "");
        saveData = string.IsNullOrEmpty(inputString) ? new SaveData() : JsonUtility.FromJson<SaveData>(inputString);
        // Debug.Log(inputString);
    }

#if UNITY_EDITOR
    private void OnApplicationQuit()
    {
        Save();
    }
#else
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Save();
        }
    }
#endif
}

[System.Serializable]
public partial class SaveData
{
    [SerializeField] public string baseInfo = "SAVE END";
}
