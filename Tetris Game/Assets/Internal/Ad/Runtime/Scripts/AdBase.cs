using System;
using Internal.Core;
using UnityEngine;

namespace AdCore
{
    
    public abstract class AdBase<T> : Lazyingleton<T> where T : MonoBehaviour
    {
        [System.NonSerialized] public LoadState LoadState = LoadState.None;
        [System.NonSerialized] protected AdType ADType;

        [System.NonSerialized] private bool _invokingForLoad = false;
        [System.NonSerialized] private int _retryAttempt;

        public delegate void AnalyticsSubscription(string id, object ad);
        public AnalyticsSubscription SubscribeToAnalytics;

        public virtual void Initialize(AnalyticsSubscription subscribeToAnalytics)
        {
            this.SubscribeToAnalytics = subscribeToAnalytics;
        }

        public virtual bool LoadAd()
        {
            if (_invokingForLoad)
            {
                CancelInvoke(nameof(LoadAd));
            }
            _invokingForLoad = false;
            return LoadState.Equals(LoadState.Loading);
        }
        
        protected void ForwardInvoke()
        {
            if (_invokingForLoad)
            {
                CancelInvoke(nameof(LoadAd));
                LoadAd();
            }
        }
        protected void InvokeForLoad()
        {
            _retryAttempt++;
            Invoke(nameof(LoadAd), Mathf.Pow(2, Math.Min(6, _retryAttempt)));
            _invokingForLoad = true;
        }
        private void IncreaseAttempts()
        {
            _retryAttempt++;
        }
        protected void ResetAttempts()
        {
            _retryAttempt = 0;
        }

        protected void JoinAnalytics(string id, object adUnitObject)
        {
            SubscribeToAnalytics(id, adUnitObject);
        }
    }

    public enum AdType
    {
        BANNER,
        INTERSTITIAL,
        REWARDED,
}
}
