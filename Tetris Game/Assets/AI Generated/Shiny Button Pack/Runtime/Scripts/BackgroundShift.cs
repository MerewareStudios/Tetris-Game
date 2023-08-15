using UnityEngine;

namespace IWI.UI
{
    [ExecuteAlways]
    public class BackgroundShift : MonoBehaviour
    {
        private static readonly int UIShift = Shader.PropertyToID("_UIShift");
        void Update()
        {
            Shader.SetGlobalFloat(UIShift, Time.unscaledTime);
        }
    }
}


