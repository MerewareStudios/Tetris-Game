using System;
using Game;
using Internal.Core;
using UnityEngine;

namespace IWI
{
    public class AdManager : Singleton<AdManager>
    {
        [SerializeField] public FakeAdBanner fakeAdBanner;
        [SerializeField] public FakeAdInterstitial fakeAdInterstitial;
        [SerializeField] public FakeAdRewarded fakeAdRewarded;
        [SerializeField] public int adTimeInterval = 180;
        [System.NonSerialized] private Data _data;
        [System.NonSerialized] private System.Action _maxSDKInitComplete;
        [System.NonSerialized] private const int ADBlockSuggestionMod = 5;
        
        void Awake()
        {
            FakeAdBanner.THIS = fakeAdBanner;
            FakeAdInterstitial.THIS = fakeAdInterstitial;
            FakeAdRewarded.THIS = fakeAdRewarded;
        }

        public void InitAdSDK(System.Action onInit = null)
        {
            _maxSDKInitComplete += onInit;
            _Data.LastTimeAdShown = (int)Time.time;

            MaxSdk.SetSdkKey("C9c4THkvTlfbzgV69g5ptFxgev2mrPMc1DWEMK60kzLN4ZDVulA3FPrwT5FlVputtGkSUtSKsTnv6aJnQAPJbT");
            MaxSdk.SetUserId(Account.Current.guid);
            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => 
                {
                    
                    FakeAdRewarded.THIS.Initialize();
                    FakeAdRewarded.THIS.OnLoadedStateChanged = (state) =>
                    {
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.REWARDED))
                        {
                            return;
                        }
                        AdBreakScreen.THIS.SetLoadState(state);
                    };
                    
                    if (_Data.removeAds)
                    {
                        _maxSDKInitComplete?.Invoke();
                        _maxSDKInitComplete = null;
                        return;
                    }
                    
                    
                    InitBanner();
                    
                    FakeAdInterstitial.THIS.Initialize();
                    FakeAdInterstitial.THIS.OnLoadedStateChanged = (state) =>
                    {
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.INTERSTITIAL))
                        {
                            return;
                        }
                        AdBreakScreen.THIS.SetLoadState(state);
                    };
                    
                    _maxSDKInitComplete?.Invoke();
                    _maxSDKInitComplete = null;
                };
        }

        public void OpenMediationDebugger()
        {
            MaxSdk.ShowMediationDebugger();
        }
        
        public void TryInterstitial()
        {
            if (_Data.removeAds)
            {
                return;
            }
            if (ONBOARDING.WEAPON_TAB.IsNotComplete())
            {
                return;
            }
            if (Time.time - _Data.LastTimeAdShown > adTimeInterval)
            {
                Debug.Log("show");
                ShowAdBreak();
            }
        }

        public void PrependInterstitial()
        {
            _Data.LastTimeAdShown = (int)(Time.time - adTimeInterval) - 1;
        }
        
        public void ShowBannerFrame()
        {
            if (_Data.removeAds)
            {
                return;
            }
            if (FakeAdBanner.THIS.Visible)
            {
                return;
            }
            if (!MaxSdk.IsInitialized())
            {
                _maxSDKInitComplete += ShowBannerFrame;
                return;
            }
            if (FakeAdBanner.THIS.CurrentLoadState.Equals(LoadState.None))
            {
                FakeAdBanner.THIS.LoadAd();
            }
            FakeAdBanner.THIS.ShowFrame();
        }

        private void ChangeBannerPosition(bool top)
        {
            FakeAdBanner.THIS.Position = top ? MaxSdkBase.BannerPosition.TopCenter : MaxSdkBase.BannerPosition.BottomCenter;
        }


        private void InitBanner()
        {
            FakeAdBanner.THIS.Initialize();
            UIManager.OnMenuModeChanged = ChangeBannerPosition;
        }
        private void DestroyBanner()
        {
            FakeAdBanner.THIS.DestroyBanner();
            UIManager.OnMenuModeChanged -= ChangeBannerPosition;
        }

        public void ShowAdBreak()
        {
            if (!MaxSdk.IsInitialized())
            {
                return;
            }
            if (FakeAdInterstitial.THIS.LoadState.Equals(LoadState.None))
            {
                FakeAdInterstitial.THIS.LoadAd();
                return;
            }
            if (!FakeAdInterstitial.THIS.Ready)
            {
                return;
            }

            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.INTERSTITIAL)
            .SetLoadState(FakeAdInterstitial.THIS.LoadState)
            .SetInfo(Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText)
            .SetVisualData(Onboarding.THIS.adBreakVisualData)
            .RemoveAdBreakButtonState(true)
            .PlusTicketState(false)
            .SetBackgroundImage(Const.THIS.skipAdBackgroundImage)
            .OnByPass(null)
            .OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    _Data.interSkipCount++;
                    _Data.LastTimeAdShown = (int)Time.time;
                    AnalyticsManager.AdData(AdBreakScreen.AdState.INTERSTITIAL, AdBreakScreen.AdInteraction.SKIP, _Data.interSkipCount);
                },
                () => Wallet.Consume(Const.Currency.OneAd))
            .OnTimesUp(() =>
            {
                _Data.interWatchCount++;
                _Data.LastTimeAdShown = (int)Time.time;

                AnalyticsManager.AdData(AdBreakScreen.AdState.INTERSTITIAL, AdBreakScreen.AdInteraction.WATCH, _Data.interSkipCount);
                
                AdBreakScreen.THIS.CloseImmediate();

                FakeAdInterstitial.THIS.Show(
                () =>
                {
                    if (!_data.removeAds && _data.InterAdInstance % ADBlockSuggestionMod == 0)
                    {
                        UIManager.THIS.ShowOffer_RemoveAds_AfterInterAd();
                    }
                    _data.InterAdInstance++;
                    GameManager.UpdateTimeScale();
                }, null);
            }, 3.5f)
            .Open(0.6f);
        }

        public static void ShowTicketAd(System.Action onReward, System.Action onClick = null)
        {
            if (!MaxSdk.IsInitialized())
            {
                return;
            }
            if (FakeAdRewarded.THIS.LoadState.Equals(LoadState.None))
            {
                FakeAdRewarded.THIS.LoadAd();
            }

            onReward += () =>
            {
                int now = (int)Time.time;
                int timeTheAdWillBeShown = AdManager.THIS._Data.LastTimeAdShown + AdManager.THIS.adTimeInterval;
                int timeUntilAd = timeTheAdWillBeShown - now;
                timeUntilAd = Mathf.Max(timeUntilAd, 40);
                AdManager.THIS._Data.LastTimeAdShown = now - AdManager.THIS.adTimeInterval + timeUntilAd;
            };
            
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.REWARDED)
            .SetLoadState(FakeAdRewarded.THIS.LoadState)
            .SetInfo(Onboarding.THIS.earnTicketText, Onboarding.THIS.cancelButtonText)
            .SetVisualData(Onboarding.THIS.rewardedAdVisualData)
            .RemoveAdBreakButtonState(false)
            .PlusTicketState(true)
            .SetBackgroundImage(Const.THIS.earnTicketBackgroundImage)
            .OnByPass(onReward)
            .OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    onClick?.Invoke();
                },
                () => true)
            .OnTimesUp(() =>
            {
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdRewarded.THIS.Show(
                    GameManager.UpdateTimeScale, 
                    onReward,
                null);
            }, 3.5f)
            .Open(0.1f);
        }

        public Data _Data
        {
            set
            {
                _data = value;
            }
            get => _data;
        }
        
        
        public static class Bypass
        {
            public static void Ads()
            {
                if (AdManager.THIS._Data.removeAds)
                {
                    return;
                }
                AdBreak();
                Banner();
            }
            private static void AdBreak()
            {
                AdManager.THIS._Data.removeAds = true;
            }
            private static void Banner()
            {
                AdManager.THIS.DestroyBanner();
            }
        }
        

        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool removeAds = false;
            [SerializeField] public int interSkipCount;
            [SerializeField] public int interWatchCount;
            [System.NonSerialized] public int LastTimeAdShown;
            [System.NonSerialized] public int InterAdInstance = 1;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                removeAds = data.removeAds;
                interSkipCount = data.interSkipCount;
                interWatchCount = data.interWatchCount;
            }
            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
