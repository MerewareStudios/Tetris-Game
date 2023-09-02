using System;
using Internal.Core;
using TMPro;
using UnityEngine;

public class ApplicationManager : Singleton<ApplicationManager>
{
    [SerializeField] public bool multiTouchEnabled = false;
    [SerializeField] public bool useNativeFrameRate = true;
    [SerializeField] public int targetFrameRate = 60;
    [SerializeField] public TextMeshProUGUI fpsText;
    [System.NonSerialized] public int fps;
    [System.NonSerialized] public float fpsTimestamp;
    static Setting HAPTIC;
    public static Setting SOUND;

    public virtual void Awake()
    {
        Input.multiTouchEnabled = multiTouchEnabled;
        
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
     Debug.unityLogger.logEnabled = false; 
#endif
        Application.targetFrameRate = useNativeFrameRate ? (int)Screen.currentResolution.refreshRateRatio.numerator : targetFrameRate;
#if UNITY_EDITOR
        Application.runInBackground = true;
#endif
        LoadSettings();

        fpsTimestamp = Time.realtimeSinceStartup;
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

    void LateUpdate()
    {
        fps++;
        if (Time.realtimeSinceStartup - fpsTimestamp > 1.0f)
        {
            fpsText.text = fps.ToString();
            fps = 0;
            fpsTimestamp = Time.realtimeSinceStartup;
        }
    }

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
