using System;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ApplicationManager : Singleton<ApplicationManager>
{
    [SerializeField] public bool multiTouchEnabled = false;
    [SerializeField] public bool useNativeFrameRate = true;
    [SerializeField] public int targetFrameRate = 60;
    [SerializeField] private ScriptableRendererFeature grabTextureFeature;
    [SerializeField] public GameObject appLabel;
    [SerializeField] public TextMeshProUGUI fpsText;
    
#if FPS
    [System.NonSerialized] private int _fps;
    [System.NonSerialized] private float _fpsTimestamp;
#endif

    public bool GrabFeatureEnabled
    {
        set => grabTextureFeature.SetActive(value);
        get => grabTextureFeature.isActive;
    } 

    public virtual void Awake()
    {
        Input.multiTouchEnabled = multiTouchEnabled;
        
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
     Debug.unityLogger.logEnabled = false; 
#endif
        Application.targetFrameRate = useNativeFrameRate ? (int)Screen.currentResolution.refreshRateRatio.value : targetFrameRate;
        
#if FPS
        _fpsTimestamp = Time.realtimeSinceStartup;
        appLabel.SetActive(true);
#else
        appLabel.SetActive(false);
#endif
        
        
        GrabFeatureEnabled = false;
    }
    
#if FPS
    void LateUpdate()
    {
        _fps++;
        if (Time.realtimeSinceStartup - _fpsTimestamp > 1.0f)
        {
            TimeSpan t = TimeSpan.FromSeconds(Time.realtimeSinceStartup);

            string stamp = string.Format("{0}:{1}:{2}", 
                t.Hours, 
                t.Minutes, 
                t.Seconds
                );
            
            
            fpsText.text = _fps.ToString() + " | " + stamp + " | (" + Application.version + " " + Const.THIS.bundleVersionCode + ")";
            _fps = 0;
            _fpsTimestamp = Time.realtimeSinceStartup;
        }
    }
#endif
    
        // LoadSettings();
    // private void LoadSettings()
    // {
    //     HAPTIC = new("HAPTIC", false);
    //     SOUND = new("SOUND", false);
    // }

    //public static void Vibrate(HapticPatterns.PresetType preset)
    //{
    //    if (HAPTIC.state)
    //    {
    //        HapticPatterns.PlayPreset(preset);
    //    }
    //}

    // static Setting HAPTIC;
    // public static Setting SOUND;

    // public class Setting
    // {
    //     public string name = "";
    //     public bool state = true;
    //     public Setting(string name, bool defaultState)
    //     {
    //         this.name = name;    
    //         this.state = GetSetting(defaultState);  
    //     }
    //     public bool Toggle()
    //     {
    //         state = !state;
    //         Save();
    //         return state;
    //     }
    //     private bool GetSetting(bool defaultValue)
    //     {
    //         return PlayerPrefs.GetInt(name, defaultValue ? 1 : 0) == 1;
    //     }
    //     private void Save()
    //     {
    //         PlayerPrefs.SetInt(name, state ? 1 : 0);
    //     }
    // }
}
