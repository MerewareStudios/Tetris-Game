using System;
using System.Collections.Generic;
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
        
        [Header("Offer Sets")]
        [SerializeField] private int adBreakOfferLevel = 5;
        [SerializeField] private int ticketOfferLevel = 5;
        [SerializeField] private List<OfferScreen.OfferType> offerTypesTicket;
        [SerializeField] private List<OfferScreen.OfferType> offerTypesAdBreak;
   
        void Awake()
        {
            FakeAdBanner.THIS = fakeAdBanner;
            FakeAdInterstitial.THIS = fakeAdInterstitial;
            FakeAdRewarded.THIS = fakeAdRewarded;
        }

        public void InitAdSDK(System.Action onInitComplete = null)
        {
            _Data.LastTimeAdShown = (int)Time.time;

            MaxSdk.SetSdkKey("C9c4THkvTlfbzgV69g5ptFxgev2mrPMc1DWEMK60kzLN4ZDVulA3FPrwT5FlVputtGkSUtSKsTnv6aJnQAPJbT");
            MaxSdk.SetUserId(Account.Current.guid);
            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => 
                {
                    onInitComplete?.Invoke();
                    
                    FakeAdRewarded.THIS.Initialize();
                    FakeAdRewarded.THIS.OnLoadedStateChanged = (state) =>
                    {
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.REWARDED))
                        {
                            return;
                        }
                        AdBreakScreen.THIS.SetLoadState(state);
                    };
                    
                    SetBannerBonuses(_Data.removeAds, false);
                    if (_Data.removeAds)
                    {
                        return;
                    }
                    
                    
                    InitBanner();
                    // InitMRec();
                    
                    FakeAdInterstitial.THIS.Initialize();
                    FakeAdInterstitial.THIS.OnLoadedStateChanged = (state) =>
                    {
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.INTERSTITIAL))
                        {
                            return;
                        }
                        AdBreakScreen.THIS.SetLoadState(state);
                    };
                };
        }

        public void OpenMediationDebugger()
        {
            MaxSdk.ShowMediationDebugger();
        }
        
        public void TryInterstitial(System.Action onSuccess)
        {
            if (_Data.removeAds)
            {
                onSuccess?.Invoke();
                // Debug.Log("remove ads");
                return;
            }
            if (ONBOARDING.UPGRADE_TAB.IsNotComplete())
            {
                onSuccess?.Invoke();
                // Debug.Log("UPGRADE_TAB not comp");
                return;
            }

            if (Time.time - _Data.LastTimeAdShown > adTimeInterval)
            {
                ShowAdBreak(onSuccess);
                return;
            }
                // Debug.Log("not yet time");
            onSuccess?.Invoke();
        }
        
        // private void InitMRec()
        // {
        //     FakeAdMREC.THIS.Initialize();
        //
        //     PiggyMenu.THIS.VisibilityChanged += visible =>
        //     {
        //         if (visible)
        //         {
        //             FakeAdMREC.THIS.Show();
        //         }
        //         else
        //         {
        //             FakeAdMREC.THIS.Hide();
        //         }
        //     };
        // }

        public void ShowBanner()
        {
            if (_Data.removeAds)
            {
                return;
            }
            FakeAdBanner.THIS.ShowAd();
        }
        public void ShowBannerOffer()
        {
            if (_Data.removeAds)
            {
                return;
            }
            if (_Data.BannerAccepted)
            {
                return;
            }
            if (!MaxSdk.IsInitialized())
            {
                return;
            }
            if (FakeAdBanner.THIS.CurrentLoadState.Equals(LoadState.None))
            {
                FakeAdBanner.THIS.LoadAd();
            }
            FakeAdBanner.THIS.ShowOffer();
        }

        private void ChangeBannerPosition(bool top)
        {
            FakeAdBanner.THIS.SetBannerPosition(top ? MaxSdkBase.BannerPosition.TopCenter : MaxSdkBase.BannerPosition.BottomCenter);
        }


        private void InitBanner()
        {
            FakeAdBanner.THIS.Initialize();
            FakeAdBanner.THIS.OnOfferAccepted = () =>
            {
                _Data.BannerAccepted = true;
                ShowBanner();
                AnalyticsManager.OnBannerEnabled();
            };
            FakeAdBanner.THIS.VisibilityChanged = (visible) =>
            {
                SetBannerBonuses(visible, true);
            };
            
            UIManager.OnMenuModeChanged = ChangeBannerPosition;
        }
        private void DestroyBanner()
        {
            _Data.BannerAccepted = false;
            FakeAdBanner.THIS.DestroyBanner();

            FakeAdBanner.THIS.OnOfferAccepted = null;
            FakeAdBanner.THIS.VisibilityChanged = null;
            UIManager.OnMenuModeChanged -= ChangeBannerPosition;
        }
        
        private void SetBannerBonuses(bool state, bool animated)
        {
            Spawner.THIS.SetNextBlockVisibility(state, animated ? 0.5f : 0.0f);
            Board.THIS.BoostingStack = state;
            Wallet.ReduceCosts = state;
        }

        public void ShowAdBreak(System.Action onFinish)
        {
            if (!MaxSdk.IsInitialized())
            {
                onFinish?.Invoke();
                // Debug.Log("not init");
                return;
            }
            if (FakeAdInterstitial.THIS.LoadState.Equals(LoadState.None))
            {
                FakeAdInterstitial.THIS.LoadAd();
                onFinish?.Invoke();
                // Debug.Log("load");
                return;
            }
            if (!FakeAdInterstitial.THIS.Ready)
            {
                onFinish?.Invoke();
                // Debug.Log("not ready");
                return;
            }


            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.INTERSTITIAL)
            .SetLoadState(FakeAdInterstitial.THIS.LoadState)
            .SetInfo(Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText)
            .SetVisualData(Onboarding.THIS.adBreakVisualData)
            .RemoveAdBreakButtonState(true)
            .PlusTicketState(false)
            .SetBackgroundImage(Const.THIS.skipAdBackgroundImage)
            .OnByPass(onFinish)
            .OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    // GameManager.GameTimeScale(1.0f);
                    onFinish?.Invoke();

                    _Data.interSkipCount++;
                    AnalyticsManager.AdData(AdBreakScreen.AdState.INTERSTITIAL, AdBreakScreen.AdInteraction.SKIP, _Data.interSkipCount);
                },
                () => Wallet.Consume(Const.Currency.OneAd))
            .OnTimesUp(() =>
            {
                _Data.interWatchCount++;
                AnalyticsManager.AdData(AdBreakScreen.AdState.INTERSTITIAL, AdBreakScreen.AdInteraction.WATCH, _Data.interSkipCount);

                
                _Data.LastTimeAdShown = (int)Time.time;
                
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdInterstitial.THIS.Show(
                () =>
                {
                    onFinish?.Invoke();

                    if (!_data.removeAds && _data.InterAdInstance % 3 == 0)
                    {
                        UIManager.THIS.ShowOffer_RemoveAds_AfterInterAd();
                    }
                    _data.InterAdInstance++;
                    GameManager.UpdateTimeScale();
                }, 
                () =>
                {
                    // AdBreakScreen.THIS.CloseImmediate();
                    onFinish?.Invoke();
                });
            }, 3.5f)
            .Open();
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
                // if (Time.time - _Data.LastTimeAdShown > AdTimeInterval)

                int now = (int)Time.time;
                int timeTheAdWillBeShown = AdManager.THIS._Data.LastTimeAdShown + AdManager.THIS.adTimeInterval;
                int timeUntilAd = timeTheAdWillBeShown - now;
                timeUntilAd = Mathf.Max(timeUntilAd, 30);
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
            .Open();
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
                AdManager.THIS.SetBannerBonuses(true, true);
                
            }
        }
        

        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool removeAds = false;
            [SerializeField] public int interSkipCount;
            [SerializeField] public int interWatchCount;
            [System.NonSerialized] public bool BannerAccepted = false;
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
