using System;

namespace Unity.Usercentrics
{

    [Serializable]
    public class BannerSettings
    {
        public GeneralStyleSettings generalStyleSettings;
        public FirstLayerStyleSettings firstLayerStyleSettings;
        public SecondLayerStyleSettings secondLayerStyleSettings = null;
        public string variantName;

        public BannerSettings(GeneralStyleSettings generalStyleSettings = null,
                              FirstLayerStyleSettings firstLayerStyleSettings = null,
                              SecondLayerStyleSettings secondLayerStyleSettings = null,
                              string variantName = "")
        {
            this.generalStyleSettings = generalStyleSettings;
            this.firstLayerStyleSettings = firstLayerStyleSettings;
            this.secondLayerStyleSettings = secondLayerStyleSettings;
            this.variantName = variantName;
        }
    }

    [Serializable]
    public class GeneralStyleSettings
    {
        public bool androidDisableSystemBackButton;
        public string androidStatusBarColor;

        internal GeneralStyleSettings() { }

        public GeneralStyleSettings(bool androidDisableSystemBackButton, string androidStatusBarColor)
        {
            this.androidDisableSystemBackButton = androidDisableSystemBackButton;
            this.androidStatusBarColor = androidStatusBarColor;
        }
    }

    [Serializable]
    public class SecondLayerStyleSettings
    {
        public bool showCloseButton;

        public SecondLayerStyleSettings(bool showCloseButton)
        {
            this.showCloseButton = showCloseButton;
        }
    }
}
