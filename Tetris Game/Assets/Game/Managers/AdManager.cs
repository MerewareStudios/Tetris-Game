using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
                
                AdBreakScreen.Set(Const.THIS.adSettings.adBreakSkipTime,
                    () =>
                    {
                        FakeAdInterstitial.Show(() =>
                        {
                            Debug.LogWarning("Fake Ad Interstitial (On Finish)");
                            UIManager.MenuMode(false);
                        });   
                    },
                    () =>
                    {
                        // Toast.Show("Ad skipped!", 1.0f);
                        UIManager.MenuMode(false);
                    }, 
                    () =>
                    {
                        bool state = Wallet.Transaction(Const.Currency.OneAdConsume);
                        if (!state)
                        {
                            // Toast.Show("No Funds!", 1.0f);
                        }

                        return state;
                    });
            }
            
            Board.THIS.OnMerge += () =>
            {
                Try2AdBreak();
                
                if (ONBOARDING.HAVE_MERGED.IsNotComplete())
                {
                    Onboarding.CheerForMerge();
                    
                    ONBOARDING.HAVE_MERGED.SetComplete();
                }
            };
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
            
            Debug.LogWarning(_Data.MergeLeftForAdBreak + " Merges Left for an Ad Break");


            _Data.mergeCountForAdBreak++;

            if (_Data.CanShowAdBreak)
            {
                _Data.mergeCountForAdBreak = 0;
                UIManager.MenuMode(true);
                AdBreakScreen.Show();
            }
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

            public bool CanShowAdBreak => mergeCountForAdBreak >= Const.THIS.adSettings.mergePerAdBreak;
            public int MergeLeftForAdBreak => Const.THIS.adSettings.mergePerAdBreak - mergeCountForAdBreak;

            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
