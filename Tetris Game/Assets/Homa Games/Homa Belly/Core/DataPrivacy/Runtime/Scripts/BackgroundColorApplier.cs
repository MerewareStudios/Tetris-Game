using UnityEngine;
using UnityEngine.UI;

namespace HomaGames.HomaBelly.DataPrivacy
{
    internal sealed class BackgroundColorApplier : MonoBehaviour
    {
        private static readonly Color LinearBackgroundColor = new Color(0.051f, 0.051f, 0.086f);
        private static readonly Color GammaBackgroundColor = new Color(0.063f, 0.063f, 0.090f);
        
        private void Awake()
        {
            var panelImage = GetComponent<Image>();
            var isLinearColorSpace = QualitySettings.activeColorSpace == ColorSpace.Linear;
            panelImage.color = isLinearColorSpace ? LinearBackgroundColor : GammaBackgroundColor;
        }
    }
}