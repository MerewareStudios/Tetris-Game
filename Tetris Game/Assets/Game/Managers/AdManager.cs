using System;
using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;


namespace IWI
{
    public class AdManager : Singleton<AdManager>
    {
        [System.NonSerialized] private Data _data;

        void Start()
        {
            FakeAdBanner.Show();
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
        }

        public Data _Data
        {
            set
            {
                _data = value;
            }
            get => _data;
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
