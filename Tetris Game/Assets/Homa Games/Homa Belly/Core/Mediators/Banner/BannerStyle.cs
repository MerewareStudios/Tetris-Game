using UnityEngine;

namespace HomaGames.HomaBelly
{
    public struct BannerStyle
    {
        public BannerStyle(BannerSize size, BannerPosition position, Color backgroundColor)
        {
            Size = size;
            Position = position;
            BackgroundColor = backgroundColor;
        }

        public BannerSize Size { get; set; }
        public BannerPosition Position { get; set; }
        public Color BackgroundColor { get; set; }
    }
}