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
        [SerializeField] public int adBreakMarchLimit = 6;
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
                    FakeAdBanner.THIS.Initialize();
                    FakeAdInterstitial.THIS.Initialize();
                    FakeAdRewarded.THIS.Initialize();
                };
        }
        
        private void Start()
        {
            InitAdSDK();
            
            FakeAdBanner.THIS.OnOfferAccepted = () =>
            {
                _Data.BannerEnabled = true;
                ShowBannerOrOffer();
            };
            FakeAdBanner.THIS.OnVisibilityChanged = (visible) =>
            {
                Spawner.THIS.NextBlockEnabled = visible;
                Board.THIS.BoostingStack = visible;
                Wallet.ReduceCosts = visible;
            };
            
            FakeAdInterstitial.THIS.OnLoadedStateChanged = (state) =>
            {
                // Debug.LogWarning("FakeAdInterstitial OnLoadedStateChanged " + state);
                if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.Interstitial))
                {
                    return;
                }
                AdBreakScreen.THIS.SetLoadState(state);
            };
            
            FakeAdRewarded.THIS.OnLoadedStateChanged = (state) =>
            {
                // Debug.LogWarning("FakeAdRewarded OnLoadedStateChanged " + state);
                if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdState.Rewarded))
                {
                    return;
                }
                AdBreakScreen.THIS.SetLoadState(state);
            };

            
            UIManager.OnMenuModeChanged += (menuVisible) =>
            {
                FakeAdBanner.THIS.SetBannerPosition(menuVisible ? MaxSdkBase.BannerPosition.TopCenter : MaxSdkBase.BannerPosition.BottomCenter);
            };
        }

        public void MarchInterstitial(System.Action onSuccess)
        {
            if (!_Data.adBreakEnabled)
            {
                onSuccess?.Invoke();
                return;
            }
            _Data.AdBreakMarch++;
            if (_Data.AdBreakMarch >= this.adBreakMarchLimit)
            {
                _Data.AdBreakMarch = 0;
                ShowAdBreak(onSuccess);
                return;
            }
            onSuccess?.Invoke();
        }

        public void ShowBannerOrOffer()
        {
            if (_Data.BannerEnabled)
            {
                FakeAdBanner.THIS.ShowAd();
            }
            else
            {
                FakeAdBanner.THIS.ShowOffer();
            }
        }

        public void CloseBanner()
        {
            _Data.BannerEnabled = false;
            FakeAdBanner.THIS.HideAd();
        }

        public void ShowAdBreak(System.Action onFinish)
        {
            if (!FakeAdInterstitial.THIS.Ready)
            {
                _Data.AdBreakMarch = adBreakMarchLimit;
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
                },
                () => Wallet.Consume(Const.Currency.OneAd));
            AdBreakScreen.THIS.OnTimesUp(() =>
            {
                AdBreakScreen.THIS.CloseImmediate();
                FakeAdInterstitial.THIS.Show(
                () =>
                {
                    this.adBreakMarchLimit++;
                    UIManager.Pause(false);
                    onFinish?.Invoke();
                }, 
                () =>
                {
                    UIManager.Pause(false);
                    onFinish?.Invoke();
                });
            }, 4);
                
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
            }, 4);
                
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

        public void Try2AdBreak()
        {
            if (!_Data.adBreakEnabled)
            {
                return;
            }
            
            // Debug.LogWarning(_Data.MergeLeftForAdBreak + " Merges Left for an Ad Break");
            _Data.AdBreakMarch++;

            // if (_Data.CanShowAdBreak)
            // {
            //     _Data.mergeCountForAdBreak = 0;
            //     UIManager.MenuMode(true);
            //     // AdBreakScreen.Show();
            // }
        }
        
// #if !UNITY_EDITOR
//     private void OnApplicationPause(bool pause)
//     {
//         if (pause)
//         {
//             // FakeAdBanner.THIS.DestroyBanner();
//         }
//         else
//         {
//             DOVirtual.DelayedCall(2.5f, () =>
//             {
//                 MaxSdk.ShowBanner(FakeAdBanner.BannerAdUnitId);
//                 FakeAdBanner.THIS.SetBannerPosition(MaxSdkBase.BannerPosition.BottomCenter);
//                 MaxSdk.ShowBanner(FakeAdBanner.BannerAdUnitId);
//             });
//         }
//     }
// #endif
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool adBreakEnabled = true;
            
            [SerializeField] public int AdBreakMarch = 0;
            [System.NonSerialized] public bool BannerEnabled = false;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                adBreakEnabled = data.adBreakEnabled;
                // bannerEnabled = data.bannerEnabled;
            }
            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
