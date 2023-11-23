using System;
using IWI.UI;
using UnityEngine;

namespace IWI.UI
{
    [CreateAssetMenu(fileName = "UI Emitter Motion Data", menuName = "UI Emitter/New Motion Data", order = 0)]
    [Serializable]
    public class MotionData : ScriptableObject
    {
        [SerializeField] public BurstSettings burstSettings;
        [SerializeField] public GeneralSettings generalSettings;

        public float MaxDuration =>
            (burstSettings.burst ? burstSettings.duration.constantMax : 0.0f)
            +(burstSettings.burst ? burstSettings.endDelay.constantMax : 0.0f)
            +generalSettings.duration.constantMax;
    }
}
