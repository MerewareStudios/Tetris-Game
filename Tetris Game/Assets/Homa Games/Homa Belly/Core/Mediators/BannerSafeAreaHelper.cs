using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Internal;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// This component helps developer design UIs with banners and Unity's safe area.
    ///
    /// When placed on a Canvas GameObject, it will take its child named "SafeArea", and resize
    /// it to fit the inside of <see cref="Screen.safeArea">Screen.safeArea</see>, while also taking
    /// in account the banners displayed on screen. 
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class BannerSafeAreaHelper : MonoBehaviour
    {
        private RectTransform _safeAreaTransform;
        private Canvas _canvas;
        private Rect _lastBannerAdjustedSafeArea;
        
        // Banner height gathering timeout
        private static readonly TimeSpan _timeSpan3Seconds = TimeSpan.FromSeconds(3);
        
        private void OnEnable() => SubscribeEvents();

        private void OnDisable() => UnSubscribeEvents();

        private void Awake()
        {
            _safeAreaTransform = transform.Find("SafeArea") as RectTransform;
            _canvas = GetComponent<Canvas>();
        }

        private void Start()
        { 
            UpdateSafeArea().ListenForErrors();
        }

        private void SubscribeEvents()
        {
            InternalAdEvents.onBannerAdShownEvent += UpdateSafeAreaCallback;
            InternalAdEvents.onBannerAdHiddenEvent += UpdateSafeAreaCallback;
            InternalAdEvents.onBannerAdDestroyedEvent += UpdateSafeAreaCallback;
        }

        private void UnSubscribeEvents()
        {
            InternalAdEvents.onBannerAdShownEvent -= UpdateSafeAreaCallback;
            InternalAdEvents.onBannerAdHiddenEvent -= UpdateSafeAreaCallback;
            InternalAdEvents.onBannerAdDestroyedEvent -= UpdateSafeAreaCallback;
        }

        private void UpdateSafeAreaCallback(AdInfo _)
        {
            UpdateSafeArea().ListenForErrors();
        }

        private async Task UpdateSafeArea()
        {
            var bannerAdjustedSafeArea = await GetBannerAdjustedSafeArea();

            if (bannerAdjustedSafeArea == _lastBannerAdjustedSafeArea)
                return;

            _lastBannerAdjustedSafeArea = bannerAdjustedSafeArea;
            SetSafeAreaRectTransform(_lastBannerAdjustedSafeArea);
        }

        private static async Task<Rect> GetBannerAdjustedSafeArea()
        {
            var safeArea = Screen.safeArea;
            
            foreach (var bannerData in HomaBelly.Instance.GetAllDisplayedBannerData())
            {
                safeArea = await RemoveBannerSpaceFromRect(safeArea, bannerData.AdInfo.PlacementId,
                    bannerData.Style.Position);
            }

            return safeArea;
        }

        private static async Task<Rect> RemoveBannerSpaceFromRect(Rect rect, string bannerPlacementId, BannerPosition bannerPosition)
        {
            float bannerHeight = await InternalGetBannerHeight(bannerPlacementId);

            switch (bannerPosition)
            {
                case BannerPosition.BOTTOM:
                    rect.yMin += bannerHeight;
                    break;
                case BannerPosition.TOP:
                    rect.yMax -= bannerHeight;
                    break;
            }
            
            return rect;
        }

        /// <summary>
        /// Asynchronously obtain visible banner height. As it may happen we are trying to obtain
        /// this metric before the banner is shown in the UI, we may receive a 0f as its height. Hence,
        /// we retry each 10ms up to 3 seconds until we have a positive value
        /// </summary>
        /// <param name="bannerPlacementId">The banner's placement ID</param>
        /// <returns>Task with the banner height</returns>
        private static async Task<float> InternalGetBannerHeight(string bannerPlacementId)
        {
            float bannerHeight = HomaBelly.Instance.GetBannerHeight(bannerPlacementId);
            using var cancellationTokenSource = new CancellationTokenSource(_timeSpan3Seconds);
            
            try
            {
                while (bannerHeight <= 0)
                {
                    await Task.Delay(10, cancellationTokenSource.Token);
                    bannerHeight = HomaBelly.Instance.GetBannerHeight(bannerPlacementId);
                }
            }
            catch (OperationCanceledException operationCanceledException)
            {
                Analytics.CustomEvent("banner_height_timeout", new Dictionary<string, object>()
                {
                    { "placement_id", bannerPlacementId }
                });
                
                HomaGamesLog.Warning($"[BannerSafe] Timeout reading banner height: {operationCanceledException}");
            }

            return bannerHeight;
        }
        
        private void SetSafeAreaRectTransform(Rect newRect)
        {
            if (! _safeAreaTransform)
                return;
            
            var anchorMin = newRect.position;
            var anchorMax = newRect.position + newRect.size;
            
            var canvasPixelRect = _canvas.pixelRect;
            anchorMin.x /= canvasPixelRect.width;
            anchorMin.y /= canvasPixelRect.height;
            anchorMax.x /= canvasPixelRect.width;
            anchorMax.y /= canvasPixelRect.height;

            _safeAreaTransform.anchorMin = anchorMin;
            _safeAreaTransform.anchorMax = anchorMax;
        }
    }
}
