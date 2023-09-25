using System;
using Game;
using Internal.Core;
using UnityEngine;

namespace IWI
{
    public class AdManager : Singleton<AdManager>
    {
        [SerializeField] private bool showAds = true;
        [System.NonSerialized] private Data _data;

        void Start()
        {
            // yield return new WaitForSeconds(0.25f);
            if (showAds)
            {
                // FakeAdBanner.Show();
                // FakeAdInterstitial.Show(() =>
                // {
                //     Debug.LogWarning("Fake Ad Interstitial (On Finish)");
                // });   
                //
                // FakeAdRewarded.Show(
                //     () =>
                //     {
                //         Debug.LogWarning("Fake Ad Rewarded (On Reward)");
                //     },
                //     () =>
                //     {
                //         Debug.LogWarning("Fake Ad Rewarded (On Skip)");
                //     }
                // );   

                // ShowAdBreak();
            }
            
            Board.THIS.OnMerge += () =>
            {
                // Try2AdBreak();
                
                if (ONBOARDING.HAVE_MERGED.IsNotComplete())
                {
                    Onboarding.CheerForMerge();
                    
                    ONBOARDING.HAVE_MERGED.SetComplete();
                }
                if (ONBOARDING.EARN_SHOP_POINT.IsComplete())
                {
                    UIManager.THIS.shop.Increase();
                }
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
            }, 5);
                
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
            }, 5);
                
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

            // public bool CanShowAdBreak => mergeCountForAdBreak >= Const.THIS.adSettings.mergePerAdBreak;
            // public int MergeLeftForAdBreak => Const.THIS.adSettings.mergePerAdBreak - mergeCountForAdBreak;

            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
