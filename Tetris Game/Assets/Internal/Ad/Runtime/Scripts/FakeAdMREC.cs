// using Internal.Core;
// using UnityEngine;
//
// public class FakeAdMREC : Lazyingleton<FakeAdMREC>
// {
//     private const string AdUnitId = "b6a166d866129b09";
//     [System.NonSerialized] public System.Action<LoadState> OnLoadedStateChanged;
//     [System.NonSerialized] public LoadState LoadState = LoadState.Loading;
//
//     public bool Ready => LoadState.Equals(LoadState.Success);
//
//     public void Show()
//     {
//         if (!Ready)
//         {
//             return;
//         }
//         MaxSdk.ShowMRec(AdUnitId);
//     }
//     public void Hide()
//     {
//         MaxSdk.HideMRec(AdUnitId);
//     }
//
//     public void Destroy()
//     {
//         MaxSdk.DestroyMRec(AdUnitId);
//     }
//     
//     public void Initialize()
//     {
//         MaxSdk.CreateMRec(AdUnitId, MaxSdkBase.AdViewPosition.Centered);
//         // MaxSdk.StopMRecAutoRefresh(AdUnitId);
//         // MaxSdk.LoadMRec(AdUnitId);
//         // MaxSdk.LoadMRec(AdUnitId);
//
//
//         MaxSdkCallbacks.MRec.OnAdLoadedEvent      += OnMRecAdLoadedEvent;
//         MaxSdkCallbacks.MRec.OnAdLoadFailedEvent  += OnMRecAdLoadFailedEvent;
//         MaxSdkCallbacks.MRec.OnAdClickedEvent     += OnMRecAdClickedEvent;
//         MaxSdkCallbacks.MRec.OnAdRevenuePaidEvent += OnMRecAdRevenuePaidEvent;
//         MaxSdkCallbacks.MRec.OnAdExpandedEvent    += OnMRecAdExpandedEvent;
//         MaxSdkCallbacks.MRec.OnAdCollapsedEvent   += OnMRecAdCollapsedEvent;
//     }
//
//     // private void LoadAd()
//     // {
//     //     LoadState = LoadState.Loading;
//     //     OnLoadedStateChanged?.Invoke(LoadState);
//     // }
//
//
//
//     public void OnMRecAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         LoadState = LoadState.Success;
//         OnLoadedStateChanged?.Invoke(LoadState);
//     }
//
//     public void OnMRecAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo error)
//     {
//         LoadState = LoadState.Fail;
//         OnLoadedStateChanged?.Invoke(LoadState);
//         // Invoke(nameof(LoadAd), Mathf.Pow(2, Math.Min(6, _retryAttempt)));
//     }
//
//     public void OnMRecAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         
//     }
//
//     public void OnMRecAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         
//     }
//
//     public void OnMRecAdExpandedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         
//     }
//
//     public void OnMRecAdCollapsedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
//     {
//         
//     }
// }
//     
