using System;
using DG.Tweening;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [SerializeField] private ParticleSystem zonePs;
    [SerializeField] private Vector3 targetShowScale;
    [SerializeField] private Vector3 targetHideScale;
    [System.NonSerialized] private Data _data;

    private bool EffectEnabled
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
        StatDisplayArranger.THIS.UpdateAmount(StatDisplay.Type.Shield, _Data.Amount, 0.5f);
        Resume();
    }
    public void AddOnly(int value)
    {
        _Data.Amount += value;
    }
    public bool Remove()
    {
        if (!_Data.Available)
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
        EffectEnabled = false;
        this.enabled = false;
        StatDisplayArranger.THIS.Hide(StatDisplay.Type.Shield);
    }
    
    public void Resume()
    {
        EffectEnabled = _Data.Available;
        this.enabled = true;
        
        if (_Data.Available)
        {
            StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, _Data.Amount, _Data.Percent, false, false);
        }
    }

    void Update()
    {
        _Data.ConsumeTime(Time.deltaTime);
        StatDisplayArranger.THIS.UpdateFill(StatDisplay.Type.Shield, _Data.Percent);

        if (!_Data.Available)
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
        public bool Available => AvailableByTime && AvailableByCount;
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
