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
        // [SerializeField] public FakeAdMREC fakeAdMRec;
        [SerializeField] public float AdTimeInterval = 180.0f;
        // [System.NonSerialized] private const int ADBreakMarchLimit = 3;
        [System.NonSerialized] private Data _data;
        

        void Awake()
        {
            FakeAdBanner.THIS = fakeAdBanner;
            FakeAdInterstitial.THIS = fakeAdInterstitial;
            FakeAdRewarded.THIS = fakeAdRewarded;
            // FakeAdMREC.THIS = fakeAdMRec;
        }

        private void InitAdSDK()
        {
            MaxSdk.SetSdkKey("C9c4THkvTlfbzgV69g5ptFxgev2mrPMc1DWEMK60kzLN4ZDVulA3FPrwT5FlVputtGkSUtSKsTnv6aJnQAPJbT");
            // MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => 
                {
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
        
        private void Start()
        {
            InitAdSDK();

            _Data.LastTimeAdShown = Time.realtimeSinceStartup;
        }

        public void TryInterstitial(System.Action onSuccess)
        {
            if (_Data.removeAds)
            {
                onSuccess?.Invoke();
                return;
            }
            if (ONBOARDING.UPGRADE_TAB.IsNotComplete())
            {
                onSuccess?.Invoke();
                return;
            }
            if (Time.realtimeSinceStartup - _Data.LastTimeAdShown > AdTimeInterval)
            {
                ShowAdBreak(onSuccess);
                return;
            }
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

        public void ShowBannerOrOffer()
        {
            if (_Data.removeAds)
            {
                return;
            }
            if (_Data.BannerAccepted)
            {
                FakeAdBanner.THIS.ShowAd();
            }
            else
            {
                FakeAdBanner.THIS.ShowOffer();
            }
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
                ShowBannerOrOffer();
                AnalyticsManager.OnBannerEnabled();
            };
            FakeAdBanner.THIS.OnVisibilityChanged = (visible) =>
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
            FakeAdBanner.THIS.OnVisibilityChanged = null;
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
            if (!FakeAdInterstitial.THIS.Ready)
            {
                onFinish?.Invoke();
                return;
            }
            
            _Data.LastTimeAdShown = Time.realtimeSinceStartup;


            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.INTERSTITIAL);
            AdBreakScreen.THIS.SetLoadState(FakeAdInterstitial.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.adBreakText,Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText);
            AdBreakScreen.THIS.SetPurchaseWindows(true, IAPManager.THIS.GetLocalPrice(UpgradeMenu.PurchaseType.REMOVE_ADS), true, IAPManager.THIS.GetLocalPrice(UpgradeMenu.PurchaseType.TICKET_PACK));
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    UIManager.Pause(false);
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
                    UIManager.Pause(false);
                    onFinish?.Invoke();
                }, 
                () =>
                {
                    UIManager.Pause(false);
                    onFinish?.Invoke();
                });
            }, onFinish, 4);
                
            UIManager.Pause(true);
            AdBreakScreen.THIS.Open();
        }

        public static void ShowTicketAd(System.Action onReward, bool pauseUnpause = true, System.Action onClick = null)
        {
            Debug.Log("req");
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.REWARDED);
            AdBreakScreen.THIS.SetLoadState(FakeAdRewarded.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.earnText,Onboarding.THIS.earnTicketText, Onboarding.THIS.cancelButtonText);
            AdBreakScreen.THIS.SetPurchaseWindows(false, "", true, IAPManager.THIS.GetLocalPrice(UpgradeMenu.PurchaseType.TICKET_PACK));
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    if (pauseUnpause)
                    {
                        UIManager.Pause(false);
                    }
                    onClick?.Invoke();
                },
                () => true);
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdRewarded.THIS.Show(
                () =>
                {
                    if (pauseUnpause)
                    {
                        UIManager.Pause(false);
                    }
                }, 
                () =>
                {
                    onReward?.Invoke();
                },
                () =>
                {
                    if (pauseUnpause)
                    {
                        UIManager.Pause(false);
                    }
                });
            }, onReward, 4);
                
            UIManager.Pause(true);
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
