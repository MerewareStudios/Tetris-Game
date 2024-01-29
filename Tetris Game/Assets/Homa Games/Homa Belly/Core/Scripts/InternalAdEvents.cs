using System;
using System.Linq;

namespace HomaGames.HomaBelly.Internal
{
    public class InternalAdEvents
    {
        /// <summary>
        /// Invoked once the banner has been shown
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onBannerAdShownEvent;

        public void OnBannerAdShownEvent(AdInfo adInfo) => onBannerAdShownEvent?.Invoke(adInfo);

        private static event Action<AdInfo> _onBannerAdHiddenEvent;

        /// <summary>
        /// Invoked once the banner has hidden
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onBannerAdHiddenEvent;

        public void OnBannerAdHiddenEvent(AdInfo adInfo) => onBannerAdHiddenEvent?.Invoke(adInfo);

        /// <summary>
        /// Invoked once the banner has destroyed
        /// <typeparam name="AdInfo">See <see cref="AdInfo"/></typeparam>
        /// </summary>
        public static event Action<AdInfo> onBannerAdDestroyedEvent;

        public void OnBannerAdDestroyedEvent(AdInfo adInfo) => onBannerAdDestroyedEvent?.Invoke(adInfo);
    }
}