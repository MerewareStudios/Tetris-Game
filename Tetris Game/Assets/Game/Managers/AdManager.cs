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
        [System.NonSerialized] private const int ADBreakMarchLimit = 3;
        [System.NonSerialized] private Data _data;
        

        void Awake()
        {
            FakeAdBanner.THIS = fakeAdBanner;
            FakeAdInterstitial.THIS = fakeAdInterstitial;
            FakeAdRewarded.THIS = fakeAdRewarded;
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
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.Rewarded))
                        {
                            return;
                        }
                        AdBreakScreen.THIS.SetLoadState(state);
                    };
                    
                    
                    if (_Data.removeAds)
                    {
                        SetBannerBonuses(true);
                        return;
                    }
                    
                    
                    InitBanner();
                    
                    
                    FakeAdInterstitial.THIS.Initialize();
                    FakeAdInterstitial.THIS.OnLoadedStateChanged = (state) =>
                    {
                        // Debug.LogWarning("FakeAdInterstitial OnLoadedStateChanged " + state);
                        if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.Interstitial))
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
        }

        public void MarchInterstitial(System.Action onSuccess)
        {
            if (_Data.removeAds)
            {
                onSuccess?.Invoke();
                return;
            }
            _Data.AdBreakMarch++;
            if (_Data.AdBreakMarch >= ADBreakMarchLimit)
            {
                _Data.AdBreakMarch = 0;
                ShowAdBreak(onSuccess);
                return;
            }
            onSuccess?.Invoke();
        }

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
                _Data.AdBreakMarch = ADBreakMarchLimit;
                return;
            }

            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.Interstitial);
            AdBreakScreen.THIS.SetLoadState(FakeAdInterstitial.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.adBreakText,Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText);
            AdBreakScreen.THIS.SetPurchaseWindows(true, true);
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    UIManager.Pause(false);
                    onFinish?.Invoke();
                },
                () => Wallet.Consume(Const.Currency.OneAd));
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
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

        public static void ShowTicketAd(System.Action onReward)
        {
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdState.Rewarded);
            AdBreakScreen.THIS.SetLoadState(FakeAdRewarded.THIS.LoadState);
            AdBreakScreen.THIS.SetInfo(Onboarding.THIS.earnText,Onboarding.THIS.earnTicketText, Onboarding.THIS.cancelButtonText);
            AdBreakScreen.THIS.SetPurchaseWindows(false, true);
            AdBreakScreen.THIS.OnClick(
                () =>
                {
                    AdBreakScreen.THIS.Close();
                    UIManager.Pause(false);
                },
                () => true);
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdRewarded.THIS.Show(
                () =>
                {
                    UIManager.Pause(false);
                }, 
                () =>
                {
                    onReward?.Invoke();
                },
                () =>
                {
                    UIManager.Pause(false);
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
            [System.NonSerialized] public bool BannerAccepted = false;
            [System.NonSerialized] public int AdBreakMarch = 0;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                removeAds = data.removeAds;
            }
            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
