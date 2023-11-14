using System;
using Game;
using Game.UI;
using Internal.Core;
using UnityEngine;

namespace IWI
{
    public class AdManager : Singleton<AdManager>
    {
        [SerializeField] public FakeAdBanner fakeAdBanner;
        [SerializeField] public FakeAdInterstitial fakeAdInterstitial;
        [SerializeField] public FakeAdRewarded fakeAdRewarded;
        [SerializeField] public float AdTimeInterval = 180.0f;
        [System.NonSerialized] private Data _data;
        

        void Awake()
        {
            FakeAdBanner.THIS = fakeAdBanner;
            FakeAdInterstitial.THIS = fakeAdInterstitial;
            FakeAdRewarded.THIS = fakeAdRewarded;
        }

        public void InitAdSDK(System.Action onInitComplete = null)
        {
            _Data.LastTimeAdShown = Time.realtimeSinceStartup;

            MaxSdk.SetSdkKey("C9c4THkvTlfbzgV69g5ptFxgev2mrPMc1DWEMK60kzLN4ZDVulA3FPrwT5FlVputtGkSUtSKsTnv6aJnQAPJbT");
            MaxSdk.SetUserId(Account.Current.guid);
            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => 
                {
                    onInitComplete?.Invoke();
                    
                    FakeAdRewarded.THIS.Initialize();
                    FakeAdRewarded.THIS.OnLoadedStateChanged = (state) =>
                    {
                        // Debug.LogWarning("FakeAdRewarded OnLoadedStateChanged " + state);
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.REWARDED))
                        {
                            return;
                        }
                        AdBreakScreen.THIS.SetLoadState(state);
                    };
                    
                    SetBannerBonuses(_Data.removeAds);
                    if (_Data.removeAds)
                    {
                        return;
                    }
                    
                    
                    InitBanner();
                    // InitMRec();
                    
                    FakeAdInterstitial.THIS.Initialize();
                    FakeAdInterstitial.THIS.OnLoadedStateChanged = (state) =>
                    {
                        // Debug.LogWarning("FakeAdInterstitial OnLoadedStateChanged " + state);
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
                // Debug.LogWarning("Interstitial - Skip - Remove Ads");

                // Debug.LogWarning("Interstitial Removed Ads");
                onSuccess?.Invoke();
                return;
            }
            if (ONBOARDING.UPGRADE_TAB.IsNotComplete())
            {
                // Debug.LogWarning("Interstitial - Skip - Upgrade Not Learned");

                // Debug.LogWarning("Interstitial Onboarding Not done");
                onSuccess?.Invoke();
                return;
            }
            // Debug.LogWarning(Time.time - _Data.LastTimeAdShown);

            if (Time.time - _Data.LastTimeAdShown > AdTimeInterval)
            {
                // Debug.LogWarning("Interstitial - Time Up Show");

                ShowAdBreak(onSuccess);
                return;
            }
            // Debug.LogWarning("Interstitial - Still Have Time");
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
                SetBannerBonuses(visible);
            };
            UIManager.OnMenuModeChanged += ChangeBannerPosition;
        }
        private void DestroyBanner()
        {
            _Data.BannerAccepted = false;
            FakeAdBanner.THIS.DestroyBanner();

            FakeAdBanner.THIS.OnOfferAccepted = null;
            FakeAdBanner.THIS.VisibilityChanged = null;
            UIManager.OnMenuModeChanged -= ChangeBannerPosition;
        }
        
        private void SetBannerBonuses(bool state)
        {
            Spawner.THIS.NextBlockEnabled = state;
            Board.THIS.BoostingStack = state;
            Wallet.ReduceCosts = state;
        }

        public void ShowAdBreak(System.Action onFinish)
        {
            if (!MaxSdk.IsInitialized())
            {
                // Debug.LogWarning("Interstitial - Not Init");

                onFinish?.Invoke();
                return;
            }
            if (FakeAdInterstitial.THIS.LoadState.Equals(LoadState.None))
            {
                // Debug.LogWarning("Interstitial - Skip None");
                FakeAdInterstitial.THIS.LoadAd();
                onFinish?.Invoke();
                return;
            }
            if (!FakeAdInterstitial.THIS.Ready)
            {
                // Debug.LogWarning("Interstitial - Skip Not Ready");
                
                onFinish?.Invoke();
                return;
            }
            
            _Data.LastTimeAdShown = Time.time;


            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.INTERSTITIAL);
            AdBreakScreen.THIS.SetLoadState(FakeAdInterstitial.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText);
            AdBreakScreen.THIS.SetVisualData(Onboarding.THIS.adBreakVisualData);
            AdBreakScreen.THIS.SetPurchaseWindows(true, IAPManager.THIS.GetLocalPrice(UpgradeMenu.PurchaseType.REMOVE_ADS), true, IAPManager.THIS.GetLocalPrice(UpgradeMenu.PurchaseType.TICKET_PACK));
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    GameManager.GameTimeScale(1.0f);
                    onFinish?.Invoke();

                    _Data.interSkipCount++;
                    AnalyticsManager.AdData(AdBreakScreen.AdState.INTERSTITIAL, AdBreakScreen.AdInteraction.SKIP, _Data.interSkipCount);
                },
                () => Wallet.Consume(Const.Currency.OneAd));
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
                _Data.interWatchCount++;
                AnalyticsManager.AdData(AdBreakScreen.AdState.INTERSTITIAL, AdBreakScreen.AdInteraction.WATCH, _Data.interSkipCount);

                AdBreakScreen.THIS.CloseImmediate();
                FakeAdInterstitial.THIS.Show(
                () =>
                {
                    GameManager.GameTimeScale(1.0f);
                    onFinish?.Invoke();
                }, 
                () =>
                {
                    GameManager.GameTimeScale(1.0f);
                    onFinish?.Invoke();
                });
            }, onFinish, 3.5f);
                
            GameManager.GameTimeScale(0.0f);
            AdBreakScreen.THIS.Open();
        }

        public static void ShowTicketAd(System.Action onReward, System.Action onClick = null)
        {
            if (!MaxSdk.IsInitialized())
            {
                Debug.LogWarning("Rewarded - Not Init");
                return;
            }
            if (FakeAdRewarded.THIS.LoadState.Equals(LoadState.None))
            {
                FakeAdRewarded.THIS.LoadAd();
            }
            
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.REWARDED);
            AdBreakScreen.THIS.SetLoadState(FakeAdRewarded.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.earnTicketText, Onboarding.THIS.cancelButtonText);
            AdBreakScreen.THIS.SetVisualData(Onboarding.THIS.rewardedAdVisualData);
            AdBreakScreen.THIS.SetPurchaseWindows(false, "", true, IAPManager.THIS.GetLocalPrice(UpgradeMenu.PurchaseType.TICKET_PACK));
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    GameManager.GameTimeScale(1.0f);
                    onClick?.Invoke();
                },
                () => true);
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdRewarded.THIS.Show(
                () =>
                {
                    GameManager.GameTimeScale(1.0f);
                }, 
                () =>
                {
                    onReward?.Invoke();
                },
                () =>
                {
                    GameManager.GameTimeScale(1.0f);
                });
            }, onReward, 3.5f);

            GameManager.GameTimeScale(0.0f);
            AdBreakScreen.THIS.Open();
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
                AdManager.THIS.SetBannerBonuses(true);
                
            }
        }
        

        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool removeAds = false;
            [SerializeField] public int interSkipCount;
            [SerializeField] public int interWatchCount;
            [System.NonSerialized] public bool BannerAccepted = false;
            [System.NonSerialized] public float LastTimeAdShown;
            
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
