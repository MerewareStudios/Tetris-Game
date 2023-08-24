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
    }
}
