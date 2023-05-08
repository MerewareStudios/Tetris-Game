using Internal.Core;
using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    [SerializeField] public int targetFrameRate = 60;
    static Setting HAPTIC;
    public static Setting SOUND;

    public virtual void Awake()
    {
        Application.targetFrameRate = targetFrameRate;
#if UNITY_EDITOR
        Application.runInBackground = true;
#endif
        LoadSettings();
    }
    private void LoadSettings()
    {
        HAPTIC = new("HAPTIC", false);
        SOUND = new("SOUND", false);
    }

    //public static void Vibrate(HapticPatterns.PresetType preset)
    //{
    //    if (HAPTIC.state)
    //    {
    //        HapticPatterns.PlayPreset(preset);
    //    }
    //}


    public class Setting
    {
        public string name = "";
        public bool state = true;
        public Setting(string name, bool defaultState)
        {
            this.name = name;    
            this.state = GetSetting(defaultState);  
        }
        public bool Toggle()
        {
            state = !state;
            Save();
            return state;
        }
        private bool GetSetting(bool defaultValue)
        {
            return PlayerPrefs.GetInt(name, defaultValue ? 1 : 0) == 1;
        }
        private void Save()
        {
            PlayerPrefs.SetInt(name, state ? 1 : 0);
        }
    }
}
