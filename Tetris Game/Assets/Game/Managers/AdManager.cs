// #define LOG
#if ADMOB_MEDIATION
using GoogleMobileAds.Api;
using System.Collections.Generic;
using GoogleMobileAds.Mediation.UnityAds.Api;
#else

#endif
using System;
using Internal.Core;
using UnityEngine;

namespace IWI
{
    public class AdManager : Singleton<AdManager>
    {
        [System.Diagnostics.Conditional("LOG")]
        private void Log(object o)
        {
            Debug.Log(o.ToString());
        }
        [System.Diagnostics.Conditional("LOG")]
        private void LogError(object o)
        {
            Debug.LogError(o.ToString());
        }
        
        [SerializeField] public FakeAdBanner fakeAdBanner;
        [SerializeField] public FakeAdInterstitial fakeAdInterstitial;
        [SerializeField] public FakeAdRewarded fakeAdRewarded;
        [SerializeField] public int adTimeInterval = 180;
        [System.NonSerialized] private Data _data;
        [System.NonSerialized] private const int ADBlockSuggestionMod = 5;
        
        void Awake()
        {
            FakeAdBanner.THIS = fakeAdBanner;
            FakeAdInterstitial.THIS = fakeAdInterstitial;
            FakeAdRewarded.THIS = fakeAdRewarded;
        }


        #region Per Mediator

        public static void PassPrivacyInfo(bool privacyAccepted, int age)
        {
            SetPrivacy(privacyAccepted);
            SetAge(age);
            
            UnityAds.SetConsentMetaData("privacy.consent", !Account.Current.IsUnderAgeForCOPPA());
            UnityAds.SetConsentMetaData("gdpr.consent", !Account.Current.IsUnderAgeForGDPR());
        }

        public static bool IsPrivacySet() => AdManager.THIS._Data.hasConsentTaken;
        public static void SetPrivacy(bool state) => AdManager.THIS._Data.hasConsentTaken = state;
        public static void SetAge(int age) => Account.Current.age = age;
        

        
        private void InitializeMediation()
        {
#if ADMOB_MEDIATION
            // TODO

            _data.MediationInitialized = false;

            MobileAds.Initialize(initStatus =>
            {
                Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();
                foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map)
                {
                    string className = keyValuePair.Key;
                    AdapterStatus status = keyValuePair.Value;
                    switch (status.InitializationState)
                    {
                        case AdapterState.NotReady:
                            // The adapter initialization did not complete.
                            Debug.Log("Adapter Status: " + className + " not ready.");
                            break;
                        case AdapterState.Ready:
                            // The adapter was successfully initialized.
                            Debug.Log("Adapter Status: " + className + " is initialized.");
                            break;
                    }
                }
                
                _data.MediationInitialized = true;
                
                WorkerThread.Current.AddJob(OnMediationInitialized);
            });
            
            MarkAdConsentByAge();
#else
            MaxSdk.SetSdkKey("C9c4THkvTlfbzgV69g5ptFxgev2mrPMc1DWEMK60kzLN4ZDVulA3FPrwT5FlVputtGkSUtSKsTnv6aJnQAPJbT");
            MaxSdk.SetUserId(Account.Current.guid);
            MaxSdk.InitializeSdk();
            MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) =>
            {
                OnMediationInitialized();
            };
#endif
        }

        private void MarkAdConsentByAge()
        {
            TagForUnderAgeOfConsent tagForUnderAgeOfConsent = Account.Current.IsUnderAgeForCOPPA() ? TagForUnderAgeOfConsent.True : TagForUnderAgeOfConsent.False;
            Debug.Log("TagForUnderAgeOfConsent : " + tagForUnderAgeOfConsent);
            RequestConfiguration requestConfiguration = new RequestConfiguration
            {
                TagForUnderAgeOfConsent = tagForUnderAgeOfConsent,
            };
            MobileAds.SetRequestConfiguration(requestConfiguration);
        }
        
        public static bool IsMediationInitialized()
        {
#if ADMOB_MEDIATION
             // TODO
             return AdManager.THIS._Data.MediationInitialized;
#else
            return MaxSdk.IsInitialized();
#endif
        }
        
        public static void SetBannerPositionByMenuState()
        {
            FakeAdBanner.THIS.Position = UIManager.MenuVisible ? FakeAdBanner.BannerPosition.TopCenter : FakeAdBanner.BannerPosition.BottomCenter;
        }
        #endregion

#if DEVELOPMENT_BUILD
        public static void OpenMediationDebugger()
        {
#if ADMOB_MEDIATION
            // TODO
            MobileAds.OpenAdInspector(error =>
            {
                Debug.LogError(error);
            });
#else
            MaxSdk.ShowMediationDebugger();
#endif
        }
        void OnGUI()
        {
            if (GUI.Button(new Rect(50, 450, 150, 50), "Mediation Debug"))
            {
                OpenMediationDebugger();
            }
        }
#endif
        
        public void InitAdSDK()
        {
            _Data.LastTimeAdShown = (int)Time.time;

            InitializeMediation();
        }


        private void OnMediationInitialized()
        {
            FakeAdRewarded.THIS.Initialize(AnalyticsManager.SubscribeRewardedAdImpressions);
            FakeAdRewarded.THIS.OnLoadedStateChanged = (state) =>
            {
                if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdType.REWARDED))
                {
                    return;
                }
                AdBreakScreen.THIS.SetLoadState(state);
            };
                    
            if (_Data.removeAds)
            {
                return;
            }
                    
            FakeAdBanner.THIS.Initialize(AnalyticsManager.SubscribeBannerAdImpressions);
            ShowBannerFrame();
                    
            FakeAdInterstitial.THIS.Initialize(AnalyticsManager.SubscribeInterstitialAdImpressions);
            FakeAdInterstitial.THIS.OnLoadedStateChanged = (state) =>
            {
                if (!AdBreakScreen.THIS.CurrentAdState.Equals(AdBreakScreen.AdType.INTERSTITIAL))
                {
                    return;
                }
                AdBreakScreen.THIS.SetLoadState(state);
            };
        }
        
        public void TryInterstitial(AdBreakScreen.AdReason adReason)
        {
            if (_Data.removeAds)
            {
                Log("Try Interstitial Failed : Removed Ads");
                return;
            }
            if (ONBOARDING.WEAPON_TAB.IsNotComplete())
            {
                Log("Try Interstitial Failed : Weapon Onboarding Not Complete");
                return;
            }
            if (Time.time - _Data.LastTimeAdShown < adTimeInterval)
            {
                Log("Try Interstitial Failed : Not Time Yet");
                return;
            }
            
#if CREATIVE
            if (!Const.THIS.creativeSettings.adsEnabled)
            {
                LogInterstitial("Try Interstitial Failed : Creatives Disabled Ads");
                return;
            }
#endif
            if (!IsMediationInitialized())
            {
                Log("Try Interstitial Failed : Mediation Not Initialized");
                return;
            }
            if (FakeAdInterstitial.THIS.LoadState.Equals(LoadState.None))
            {
                Log("Try Interstitial Failed : Interstitial Not Loaded");
                FakeAdInterstitial.THIS.LoadAd();
                return;
            }
            if (!FakeAdInterstitial.THIS.Ready)
            {
                Log("Try Interstitial Failed : Interstitial Not Ready");
                return;
            }
            
            ShowAdBreak(adReason);
        }

        public void PrependInterstitial()
        {
            _Data.LastTimeAdShown = (int)(Time.time - adTimeInterval) - 1;
        }
        
        public void ShowBannerFrame()
        {
            if (_Data.removeAds)
            {
                return;
            }
            if (ONBOARDING.WEAPON_TAB.IsNotComplete())
            {
                return;
            }
            if (FakeAdBanner.THIS.Visible)
            {
                return;
            }
            if (!IsMediationInitialized())
            {
                return;
            }
            if (FakeAdBanner.THIS.CurrentLoadState.Equals(LoadState.None))
            {
                AdjustBannerPosition();
                FakeAdBanner.THIS.LoadAd();
            }
            FakeAdBanner.THIS.ShowFrame();
        }


        private void DestroyBanner()
        {
            FakeAdBanner.THIS.DestroyBanner();
        }

        public void AdjustBannerPosition()
        {
            if (_Data.removeAds)
            {
                return;
            }
            SetBannerPositionByMenuState();
        }

        private static void ShowAdBreak(AdBreakScreen.AdReason adReason)
        {
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdType.INTERSTITIAL)
            .SetLoadState(FakeAdInterstitial.THIS.LoadState)
            .SetInfo(Onboarding.THIS.useTicketText, Onboarding.THIS.skipButtonText)
            .SetVisualData(Onboarding.THIS.adBreakVisualData)
            .RemoveAdBreakButtonState(true)
            .PlusTicketState(false)
            .SetBackgroundImage(Const.THIS.skipAdBackgroundImage)
            .OnByPass(null)
            .OnClick(
                () =>
                {
                    // HapticManager.OnClickVibrate(Audio.Button_Click_Exit);
                    AdBreakScreen.THIS.Close();
                    AdManager.THIS._Data.interSkipCount++;
                    AdManager.THIS._Data.LastTimeAdShown = (int)Time.time;
                    AnalyticsManager.AdData(AdBreakScreen.AdType.INTERSTITIAL, AdBreakScreen.AdInteraction.SKIP, adReason, AdManager.THIS._Data.interSkipCount);
                },
                () =>
                {
                    bool canSkip = Wallet.Consume(Const.Currency.OneAd);
                    HapticManager.OnClickVibrate(canSkip ? Audio.Button_Click_Exit : Audio.Button_Click_Forbidden);
                    return canSkip;
                })
            .OnTimesUp(() =>
            {
                AdManager.THIS._Data.interWatchCount++;
                AdManager.THIS._Data.LastTimeAdShown = (int)Time.time;

                AnalyticsManager.AdData(AdBreakScreen.AdType.INTERSTITIAL, AdBreakScreen.AdInteraction.WATCH, adReason, AdManager.THIS._Data.interWatchCount);
                
                AdBreakScreen.THIS.CloseImmediate();

                FakeAdInterstitial.THIS.Show(
                () =>
                {
                    if (!AdManager.THIS._Data.removeAds && AdManager.THIS._Data.InterAdInstance % ADBlockSuggestionMod == 0)
                    {
                        UIManager.THIS.ShowOffer_RemoveAds_AfterInterAd();
                    }
                    AdManager.THIS._Data.InterAdInstance++;
                    GameManager.UpdateTimeScale();
                }, null);
            }, 3.5f)
            .Open(0.6f);
        }

        public static void ShowTicketAd(AdBreakScreen.AdReason adReason, System.Action onReward, System.Action onClick = null)
        {
#if CREATIVE
            if (!Const.THIS.creativeSettings.adsEnabled)
            {
                return;
            }
#endif
            if (!IsMediationInitialized())
            {
                return;
            }
            if (FakeAdRewarded.THIS.LoadState.Equals(LoadState.None))
            {
                FakeAdRewarded.THIS.LoadAd();
            }

            onReward += () =>
            {
                int now = (int)Time.time;
                int timeTheAdWillBeShown = AdManager.THIS._Data.LastTimeAdShown + AdManager.THIS.adTimeInterval;
                int timeUntilAd = timeTheAdWillBeShown - now;
                timeUntilAd = Mathf.Max(timeUntilAd, 40);
                AdManager.THIS._Data.LastTimeAdShown = now - AdManager.THIS.adTimeInterval + timeUntilAd;
            };
            
            AdBreakScreen.THIS.SetAdState(AdBreakScreen.AdType.REWARDED)
            .SetLoadState(FakeAdRewarded.THIS.LoadState)
            .SetInfo(Onboarding.THIS.earnTicketText, Onboarding.THIS.cancelButtonText)
            .SetVisualData(Onboarding.THIS.rewardedAdVisualData)
            .RemoveAdBreakButtonState(false)
            .PlusTicketState(true)
            .SetBackgroundImage(Const.THIS.earnTicketBackgroundImage)
            .OnByPass(onReward)
            .OnClick(
                () =>
                {
                    HapticManager.OnClickVibrate(Audio.Button_Click_Exit);
                    AdBreakScreen.THIS.Close();
                    onClick?.Invoke();
                },
                () =>
                {
                    HapticManager.OnClickVibrate();
                    return true;
                })
            .OnTimesUp(() =>
            {
                AdManager.THIS._Data.rewardWatchCount++;
                AnalyticsManager.AdData(AdBreakScreen.AdType.REWARDED, AdBreakScreen.AdInteraction.WATCH, adReason, AdManager.THIS._Data.rewardWatchCount);

                AdBreakScreen.THIS.CloseImmediate();
                
                FakeAdRewarded.THIS.Show(
                    GameManager.UpdateTimeScale, 
                    onReward,
                null);
            }, 3.5f)
            .Open(0.1f);
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
            }
        }
        

        [System.Serializable]
        public class Data : ICloneable
        {
            [System.NonSerialized] public bool MediationInitialized = false;
            [SerializeField] public bool hasConsentTaken = false;
            [SerializeField] public bool removeAds = false;
            [SerializeField] public int interSkipCount;
            [SerializeField] public int interWatchCount;
            [SerializeField] public int rewardWatchCount;
            [System.NonSerialized] public int LastTimeAdShown;
            [System.NonSerialized] public int InterAdInstance = 1;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                hasConsentTaken = data.hasConsentTaken;
                removeAds = data.removeAds;
                interSkipCount = data.interSkipCount;
                interWatchCount = data.interWatchCount;
                rewardWatchCount = data.rewardWatchCount;
            }
            public object Clone()
            {
                return new Data(this);
            }
        } 
    }
}
