using Internal.Core;
using UnityEngine;

public class SaveManagerBase<T> : Singleton<T> where T : MonoBehaviour
{
    [SerializeField] public SaveData saveData;
    [SerializeField] public bool DELETE_AT_START;

    virtual public void Awake()
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
        Debug.Log("Saved");
        if (!outputString.Equals(""))
        {
            PlayerPrefs.SetString(nameof(SaveData), outputString);
        }
    }
    public void Load()
    {
        string inputString = PlayerPrefs.GetString(nameof(SaveData), "");
        if (inputString.Equals(""))
        {
            saveData = new SaveData();
        }
        else
        {
            // Debug.Log(inputString);
            saveData = JsonUtility.FromJson<SaveData>(inputString);
        }
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
