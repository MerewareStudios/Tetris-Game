using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LockedMiniOffer : MonoBehaviour
{
    [SerializeField] private GameObject visualParent;
    [SerializeField] private TextMeshProUGUI centerText;
    [SerializeField] private TextMeshProUGUI endStamp;
    [SerializeField] private Image fill;
    [SerializeField] private MiniOffer miniOffer;
    [SerializeField] private int durationSec;
    [SerializeField] private int increment = 10;
    [SerializeField] private int availableLevel = 7;
    [TextArea] [SerializeField] private string prefix;
    [System.NonSerialized] private Coroutine _timeCoroutine = null;
    [SerializeField] public OfferScreen.OfferType[] offerList;
    [SerializeField] public OfferScreen.AdPlacement adPlacement;

    public delegate int GetCurrentFunc();
    public GetCurrentFunc GetCurrent;

    [field: System.NonSerialized] public Data SavedData { get; set; }

    public void Set()
    {
        bool available = GetCurrent() >= availableLevel;
        this.gameObject.SetActive(available);
        if (!available)
        {
            return;
        }
        
        if (!SavedData.enabled)
        {
            if (GetCurrent() >= SavedData.unlockedAt)
            {
                EnableOffer();
                ShowOffer();
                return;
            }
            SetupOfferProgress();
            return;
        }
        if (SavedData.OfferEnded)
        {
            IncrementOffer();
            SetupOfferProgress();
            return;
        }

        ShowOffer();
    }

    
    private void SetupOfferProgress()
    {
        miniOffer.gameObject.SetActive(false);
        visualParent.SetActive(true);
        endStamp.gameObject.SetActive(false);

        fill.DOKill();
        fill.fillAmount = 0.0f;
        fill.DOFillAmount(1.0f - (GetCurrent() - SavedData.startAt) / (float)SavedData.Dif, 0.3f).SetDelay(0.25f).SetEase(Ease.OutSine).SetUpdate(true);
            //     .onComplete =
            // () =>
            // {
            //     if (GetCurrent() >= SavedData.unlockedAt)
            //     {
            //         EnableOffer();
            //         ShowOffer();
            //         return;
            //     }
            // };
        centerText.text = prefix + SavedData.unlockedAt;
        // centerText.text = SavedData.unlockedAt.ToString();
    }
    private void ShowOffer()
    {
        miniOffer.gameObject.SetActive(true);
        visualParent.SetActive(false);
        endStamp.gameObject.SetActive(true);
        
        miniOffer.ShowOffer(offerList[SavedData.offerIndex % offerList.Length], adPlacement);
        
        StopTimer();
        _timeCoroutine = StartCoroutine(TimerRoutine());
        return;

        IEnumerator TimerRoutine()
        {
            while (true)
            {
                endStamp.text = SavedData.CurrentStamp.ToString(@"mm\:ss");
                if (SavedData.OfferEnded)
                {
                    IncrementOffer();
                    SetupOfferProgress();
                    yield break;
                }
                yield return new WaitForSecondsRealtime(1);
            }
        }
    }

    private void IncrementOffer()
    {
        SavedData.startAt = GetCurrent();
        SavedData.unlockedAt = SavedData.startAt + increment;
        SavedData.offerIndex++;
        SavedData.enabled = false;
    }
    private void EnableOffer()
    {
        SavedData.enabled = false;
        SavedData.offerEnds = DateTime.Now.Ticks + TimeSpan.FromSeconds(durationSec).Ticks;
    }
    
    public void Pause()
    {
        fill.DOKill();
        StopTimer();
    }
    private void StopTimer()
    {
        if (_timeCoroutine == null)
        {
            return;
        }
        StopCoroutine(_timeCoroutine);
        _timeCoroutine = null;
    }
    
    [System.Serializable]
    public class Data : System.ICloneable
    {
        [SerializeField] public int startAt = 0;
        [SerializeField] public int unlockedAt = 15;
        [SerializeField] public int offerIndex;
        [SerializeField] public bool enabled = false;
        [SerializeField] public long offerEnds;
            
        public TimeSpan CurrentStamp => TimeSpan.FromTicks(offerEnds - DateTime.Now.Ticks);
        public bool OfferEnded => CurrentStamp.Ticks < 0;
        public int Dif => unlockedAt - startAt;

        
        public Data()
        {
                
        }
        public Data(Data data)
        {
            this.startAt = data.startAt;
            this.unlockedAt = data.unlockedAt;
            this.offerIndex = data.offerIndex;
            this.enabled = data.enabled;
            this.offerEnds = data.offerEnds;
        }

        public object Clone()
        {
            return new Data(this);
        }
    }
}
