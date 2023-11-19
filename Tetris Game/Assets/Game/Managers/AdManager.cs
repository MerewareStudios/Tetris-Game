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
                return;
            }
            if (ONBOARDING.UPGRADE_TAB.IsNotComplete())
            {
                onSuccess?.Invoke();
                return;
            }

            if (Time.time - _Data.LastTimeAdShown > AdTimeInterval)
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
                onFinish?.Invoke();
                return;
            }
            if (FakeAdInterstitial.THIS.LoadState.Equals(LoadState.None))
            {
                FakeAdInterstitial.THIS.LoadAd();
                onFinish?.Invoke();
                return;
            }
            if (!FakeAdInterstitial.THIS.Ready)
            {
                onFinish?.Invoke();
                return;
            }
            
            _Data.LastTimeAdShown = Time.time;


            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.INTERSTITIAL);
            AdBreakScreen.THIS.SetLoadState(FakeAdInterstitial.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText);
            AdBreakScreen.THIS.SetVisualData(Onboarding.THIS.adBreakVisualData);
            AdBreakScreen.THIS.RemoveAdBreakButtonState(true);
            AdBreakScreen.THIS.PlusTicketState(false);
            AdBreakScreen.THIS.SetBackgroundImage(Const.THIS.skipAdBackgroundImage);
            AdBreakScreen.THIS.OnByPass(onFinish);
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    // GameManager.GameTimeScale(1.0f);
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
                    // GameManager.GameTimeScale(1.0f);
                    onFinish?.Invoke();
                }, 
                () =>
                {
                    // GameManager.GameTimeScale(1.0f);
                    onFinish?.Invoke();
                });
            }, 3.5f);
                
            // GameManager.GameTimeScale(0.0f);
            AdBreakScreen.THIS.Open();
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
                AdManager.THIS._Data.LastTimeAdShown += 30;
            };
            
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.REWARDED);
            AdBreakScreen.THIS.SetLoadState(FakeAdRewarded.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.earnTicketText, Onboarding.THIS.cancelButtonText);
            AdBreakScreen.THIS.SetVisualData(Onboarding.THIS.rewardedAdVisualData);
            AdBreakScreen.THIS.RemoveAdBreakButtonState(false);
            AdBreakScreen.THIS.PlusTicketState(true);
            AdBreakScreen.THIS.SetBackgroundImage(Const.THIS.earnTicketBackgroundImage);
            AdBreakScreen.THIS.OnByPass(onReward);
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    // GameManager.GameTimeScale(1.0f);
                    onClick?.Invoke();
                },
                () => true);
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdRewarded.THIS.Show(
                () =>
                {
                    // GameManager.GameTimeScale(1.0f);
                }, 
                onReward,
                () =>
                {
                    // GameManager.GameTimeScale(1.0f);
                });
            }, 3.5f);

            // GameManager.GameTimeScale(0.0f);
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
        
        public static class Offers
        {
            public static void RemoveAds()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.RemoveAds);
            }
            public static void CoinPack()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.CoinPack);
            }
            public static void PiggyPack()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.PiggyCoinPack);
            }
            public static void TicketPack()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.TicketPack);
            }
            public static void HealthPack()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.HealthPack);
            }
            
            public static void Offer1()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.OfferPack1);
     
            }
            
            public static void Offer2()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.OfferPack2);
     
            }
            
            public static void Offer3()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.OfferPack3);
     
            }
            
            public static void Offer4()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.OfferPack4);
     
            }
            
            public static void Offer5()
            {
                OfferScreen.THIS.Open(OfferScreen.OfferType.OfferPack5);
     
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
