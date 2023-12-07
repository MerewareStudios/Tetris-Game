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
        Save(saveData);
    }

    public void Save(SaveData data)
    {
        Save(JsonUtility.ToJson(data));
    }
    public void Save(string str)
    {
        if (!string.IsNullOrEmpty(str))
        {
            PlayerPrefs.SetString(nameof(SaveData), str);
        }
    }

    private void Load()
    {
        string inputString = PlayerPrefs.GetString(nameof(SaveData), "");
        saveData = string.IsNullOrEmpty(inputString) ? null : JsonUtility.FromJson<SaveData>(inputString);
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
public partial class SaveData : System.ICloneable
{
    // [SerializeField] public string baseInfo = "SAVE END";
}

