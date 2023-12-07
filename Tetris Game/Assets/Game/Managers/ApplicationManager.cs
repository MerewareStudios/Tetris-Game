using Internal.Core;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ApplicationManager : Singleton<ApplicationManager>
{
    [SerializeField] public bool multiTouchEnabled = false;
    [SerializeField] public bool useNativeFrameRate = true;
    [SerializeField] public int targetFrameRate = 60;
    [SerializeField] private ScriptableRendererFeature grabTextureFeature;
    
#if FPS
    [System.NonSerialized] private int _fps;
    [System.NonSerialized] private float _fpsTimestamp;
    [SerializeField] public TextMeshProUGUI fpsText;
    [SerializeField] public GameObject appLabel;
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
}
