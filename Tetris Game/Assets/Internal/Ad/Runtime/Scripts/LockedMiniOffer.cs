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
    [SerializeField] public OfferScreen.OfferType[] offerListWithAds;
    [SerializeField] public OfferScreen.OfferType[] offerListWithoutAds;

    public delegate int GetCurrentFunc();
    public delegate bool GetAdStateFunc();
    public GetCurrentFunc GetCurrent;
    public GetAdStateFunc GetAdState;

    [field: System.NonSerialized] public Data SavedData { get; set; }

    public void Set()
    {
        if (GetCurrent() < availableLevel)
        {
            this.gameObject.SetActive(false);
            this.miniOffer.gameObject.SetActive(false);
            return;
        }
        this.gameObject.SetActive(true);

        if (SavedData.timerRunning)
        {
            ShowOffer();
            return;
        }

        SetupOfferProgress();
    }

    
    private void SetupOfferProgress()
    {
        miniOffer.gameObject.SetActive(false);
        visualParent.SetActive(true);
        endStamp.gameObject.SetActive(false);

        fill.DOKill();
        fill.fillAmount = 0.0f;
        fill.DOFillAmount((GetCurrent() - SavedData.startAt) / (float)SavedData.Dif, 0.5f).SetDelay(0.25f).SetEase(Ease.Linear).SetUpdate(true)
            .onComplete = () =>
        {
            if (GetCurrent() >= SavedData.unlockedAt)
            {
                EnableOffer();
                ShowOffer();
            }
        };
        centerText.text = prefix + SavedData.unlockedAt;
    }
    private void ShowOffer()
    {
        visualParent.SetActive(false);

        SavedData.currentOfferType = GetNextOffer();
        miniOffer.ShowOffer(SavedData.currentOfferType, OfferScreen.AdPlacement.MENUMINI);
        
        endStamp.gameObject.SetActive(true);
        
        StopTimer();
        _timeCoroutine = StartCoroutine(TimerRoutine());
        return;

        IEnumerator TimerRoutine()
        {
            while (SavedData.timerRunning)
            {
                if (SavedData.OfferEnded)
                {
                    IncrementOffer();
                    SetupOfferProgress();
                    yield break;
                }
                endStamp.text = SavedData.CurrentStamp.ToString(@"mm\:ss");
                yield return new WaitForSecondsRealtime(1);
            }
        }
    }

    public void ForceEndOfferVia(OfferScreen.OfferType offerType)
    {
        if (SavedData.currentOfferType.Equals(offerType))
        {
            SavedData.offerEnds = TimeSpan.FromSeconds(durationSec).Ticks;
        }   
    }

    private OfferScreen.OfferType GetNextOffer()
    {
        OfferScreen.OfferType[] offerTypes = GetAdState() ? this.offerListWithoutAds : offerListWithAds;
        OfferScreen.OfferType offerType = offerTypes[SavedData.offerIndex % offerTypes.Length];
        return offerType;
    }

    private void IncrementOffer()
    {
        SavedData.startAt = GetCurrent();
        SavedData.unlockedAt = SavedData.startAt + increment;
        SavedData.offerIndex++;
        SavedData.timerRunning = false;
    }
    private void EnableOffer()
    {
        SavedData.timerRunning = true;
        SavedData.offerEnds = DateTime.Now.Ticks + TimeSpan.FromSeconds(durationSec).Ticks;
    }
    
    public void Pause()
    {
        fill.DOKill();
        StopTimer();
        miniOffer.Halt();
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
        [SerializeField] public int unlockedAt;
        [SerializeField] public int offerIndex;
        [SerializeField] public bool timerRunning = false;
        [SerializeField] public long offerEnds;
        [SerializeField] public OfferScreen.OfferType currentOfferType;
            
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
            this.timerRunning = data.timerRunning;
            this.offerEnds = data.offerEnds;
            this.currentOfferType = data.currentOfferType;
        }

        public object Clone()
        {
            return new Data(this);
        }
    }
}
