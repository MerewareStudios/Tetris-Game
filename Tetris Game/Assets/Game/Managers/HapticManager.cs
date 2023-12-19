using Internal.Core;
using Lofelt.NiceVibrations;
using UnityEngine;

public class HapticManager : Singleton<HapticManager>
{
    private const string HapticsEnabled = "HAPTICS_ENABLED";

    [System.NonSerialized] public HapticManager.Data SavedData;
    
    [System.Diagnostics.Conditional(HapticsEnabled)]
    public static void Vibrate(HapticPatterns.PresetType presetType)
    {
        if (!HapticManager.THIS.SavedData.canVibrate)
        {
            return;
        }
        HapticPatterns.PlayPreset(presetType);
    }
    
    [System.Diagnostics.Conditional(HapticsEnabled)]
    public static void OnClickVibrate(Audio audio = Audio.Button_Click_Navigation)
    {
        audio.Play();
        HapticManager.Vibrate(HapticPatterns.PresetType.Warning);
    }
    
    
    [System.Serializable]
    public class Data : System.ICloneable
    {
        [SerializeField] public bool canVibrate = true;
        [SerializeField] public bool canPlayAudio = true;

        public Data()
        {
                
        }
        public Data(Data data)
        {
            canVibrate = data.canVibrate;
            canPlayAudio = data.canPlayAudio;
        }   
        public object Clone()
        {
            return new Data(this);
        }
    } 
}
