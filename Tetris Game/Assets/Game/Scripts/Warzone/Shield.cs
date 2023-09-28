using DG.Tweening;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private ParticleSystem zonePs;
    [SerializeField] private Vector3 targetShowScale;
    [SerializeField] private Vector3 targetHideScale;
    [System.NonSerialized] private Data _data;

    public bool ShieldEnabled
    {
        set
        {
            if (zonePs.isPlaying == value)
            {
                return;
            }
            if (value)
            {
                zonePs.Play();
                zonePs.transform.DOKill();
                zonePs.transform.DOScale(targetShowScale, 0.35f).SetEase(Ease.OutBack);
            }
            else
            {
                zonePs.transform.DOKill();
                zonePs.transform.DOScale(targetHideScale, 0.35f).SetEase(Ease.InBack).onComplete += () =>
                {
                    zonePs.Stop();
                };
            }
        }
        get => zonePs.isPlaying;
    }
    
    public Data _Data
    {
        set
        {
            _data = value;
        }
        get => _data;
    }

    public void Add(int value)
    {
        _Data.Amount += value;
        // StatDisplayArranger.THIS.UpdateAmount(StatDisplay.Type.Shield, _Data.Amount, 0.5f);
        Resume();
    }
    public void AddOnly(int value)
    {
        _Data.Amount += value;
        StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, _Data.Amount, _Data.Percent, false);
    }
    public bool Remove()
    {
        if (!_Data.Protecting)
        {
            return false;
        }
        _Data.Amount -= 1;
        StatDisplayArranger.THIS.UpdateAmount(StatDisplay.Type.Shield, _Data.Amount, -0.25f);
        if (!_Data.AvailableByCount)
        {
            StatDisplayArranger.THIS.Hide(StatDisplay.Type.Shield);
        }
        return true;
    }

    public void Pause()
    {
        this.enabled = false;
    }
    
    public void Stop()
    {
        ShieldEnabled = false;
        this.enabled = false;
        StatDisplayArranger.THIS.Hide(StatDisplay.Type.Shield);
    }
    
    public void Resume()
    {
        ShieldEnabled = _Data.Protecting;
        this.enabled = true;
        
        if (_Data.Protecting)
        {
            StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, _Data.Amount, _Data.Percent, false);
        }
    }

    void Update()
    {
        _Data.ConsumeTime(Time.deltaTime);
        StatDisplayArranger.THIS.UpdatePercent(StatDisplay.Type.Shield, _Data.Percent);

        if (!_Data.Protecting)
        {
            Stop();
        }
    }
    
    [System.Serializable]
    public class Data : System.ICloneable
    {
        [System.NonSerialized] private const float MaxTime = 30.0f;
        [SerializeField] public float timeLeft = 0.0f;
        [SerializeField] private int amount;

        public int Amount
        {
            set
            {
                this.amount = value;
                this.amount = Mathf.Max(amount, 0);
                if (value > 0)
                {
                    ReplenishTime();
                }
            }
            get => amount;
        }

        public float Percent => timeLeft / MaxTime;
        public bool Protecting => AvailableByTime && AvailableByCount;
        public bool AvailableByTime => timeLeft > 0.0f;
        public bool AvailableByCount => amount > 0;

        public void ReplenishTime()
        {
            timeLeft = MaxTime;
        }
        public void ConsumeTime(float duration)
        {
            timeLeft = Mathf.Max(timeLeft - duration, 0.0f);
        }
        public Data(Shield.Data data)
        {
            this.timeLeft = data.timeLeft;
            this.amount = data.amount;
        }

        public object Clone()
        {
            return new Shield.Data(this);
        }
    }
}
