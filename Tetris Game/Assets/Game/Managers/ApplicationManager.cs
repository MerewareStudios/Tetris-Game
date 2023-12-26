using System;
using Internal.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ApplicationManager : Singleton<ApplicationManager>
{
    [SerializeField] public bool multiTouchEnabled = false;
    // [SerializeField] public bool useNativeFrameRate = true;
    // [SerializeField] public int targetFrameRate = 60;
    [SerializeField] private ScriptableRendererFeature grabTextureFeature;
    [SerializeField] public AudioListener audioListener;

#if FPS
    [System.NonSerialized] private int _fps;
    [System.NonSerialized] private float _fpsTimestamp;
    [System.NonSerialized] private string fps;
#endif

    public bool GrabFeatureEnabled
    {
        set => grabTextureFeature.SetActive(value);
        get => grabTextureFeature.isActive;
    } 

    public virtual void Awake()
    {
        GrabFeatureEnabled = false;
#if CREATIVE
        GameManager.THIS.Init();
        if (Const.THIS.creativeSettings.fingerEnabled)
        {
            const string path = "Assets/Internal/Tutorial/Runtime/Prefabs/Creative Finger.prefab";
            CreativeFinger creativeFinger = MonoBehaviour.Instantiate(AssetDatabase.LoadAssetAtPath<CreativeFinger>(path));
            creativeFinger.SetUp(CameraManager.THIS.uiCamera);
        }
        Application.targetFrameRate = 60;
        return;
#endif
        Input.multiTouchEnabled = multiTouchEnabled;
        
#if !(DEVELOPMENT_BUILD || UNITY_EDITOR)
     Debug.unityLogger.logEnabled = false; 
#endif
        // Application.targetFrameRate = useNativeFrameRate ? (int)Screen.currentResolution.refreshRateRatio.value : targetFrameRate;
        // Application.targetFrameRate = targetFrameRate;
        // Application.targetFrameRate = 30;
        
#if FPS
        _fpsTimestamp = Time.realtimeSinceStartup;
#endif
    }

    private void Start()
    {
        // var audioConfiguration = AudioSettings.GetConfiguration();
        // audioConfiguration.dspBufferSize = 64;
        // AudioSettings.Reset(audioConfiguration);
        Application.targetFrameRate = 60;

    }

#if FPS
    private void OnGUI()
    {
        GUI.Label(new Rect(20, 20, 100, 40), fps);
    }

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
            
            // Application.targetFrameRate = targetFrameRate;

            fps = _fps.ToString() + " | " + stamp + " | (" + Application.version + ")";

            _fps = 0;
            _fpsTimestamp = Time.realtimeSinceStartup;
        }
    }
#endif
}
