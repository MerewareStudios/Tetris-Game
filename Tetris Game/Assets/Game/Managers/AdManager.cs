using System;
using System.Collections;
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
            MaxSdkCallbacks.OnSdkInitializedEvent += 
                (MaxSdkBase.SdkConfiguration sdkConfiguration) => 
                {
                    // AppLovin SDK is initialized, start loading ads
                };

            MaxSdk.SetSdkKey("C9c4THkvTlfbzgV69g5ptFxgev2mrPMc1DWEMK60kzLN4ZDVulA3FPrwT5FlVputtGkSUtSKsTnv6aJnQAPJbT");
            // MaxSdk.SetUserId("USER_ID");
            MaxSdk.InitializeSdk();
            
        }
        
        private void Start()
        {
            InitAdSDK();

            FakeAdBanner.THIS.Initialize();
            FakeAdBanner.THIS.ShowAd();
            // FakeAdBanner.THIS.OnAdLoadedInternal = ShowBanner;
            FakeAdBanner.THIS.OnOfferAccepted = () =>
            {
                _Data.bannerEnabled = true;
                ShowBanner();
                Spawner.THIS.NextBlockEnabled = true;
            };
            
            
            Board.THIS.OnMerge += (amount) =>
            {
                
            };
            
            UIManager.OnMenuModeChanged += (menuVisible) =>
            {
                SetBannerVisibility(!menuVisible);
            };
        }

        public void ShowBanner()
        {
            if (_Data.bannerEnabled)
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
            _Data.bannerEnabled = false;
            FakeAdBanner.THIS.HideAd();
            Spawner.THIS.NextBlockEnabled = false;
        }
        
        public void SetBannerVisibility(bool visible)
        {
            if (visible && _Data.bannerEnabled)
            {
                FakeAdBanner.THIS.ShowAd();
            }
            else
            {
                FakeAdBanner.THIS.HideAd();
            }
        }

        public static void ShowAdBreak()
        {
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
                FakeAdInterstitial.Show(() =>
                {
                    Debug.LogWarning("Fake Ad Interstitial (On Finish)");
                    UIManager.Pause(false);
                });
            }, 4);
                
            UIManager.Pause(true);
            AdBreakScreen.THIS.Open();
        }

        public static void ShowTicketAd(System.Action onReward)
        {
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
                FakeAdInterstitial.Show(() =>
                {
                    Debug.LogWarning("Fake Ad Interstitial (On Finish)");
                    onReward?.Invoke();
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
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool adBreakEnabled = true;
            [System.NonSerialized] public int AdBreakMarch = 0;
            [SerializeField] public bool bannerEnabled = false;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                adBreakEnabled = data.adBreakEnabled;
                bannerEnabled = data.bannerEnabled;
            }
            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
