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
            FakeAdBanner.Show();
            
            
            Board.THIS.OnMerge += (amount) =>
            {
                // Try2AdBreak();
            };
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
            _Data.mergeCountForAdBreak++;

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
            [SerializeField] public int mergeCountForAdBreak = 0;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                adBreakEnabled = data.adBreakEnabled;
                mergeCountForAdBreak = 0;
            }
            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
