using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class Shield : MonoBehaviour, TickManager.ITickable
{
    [SerializeField] private MeshRenderer frontLineMR;
    [SerializeField] private ParticleSystem zonePs;
    [SerializeField] private Vector3 targetShowScale;
    [SerializeField] private Vector3 targetHideScale;
    [System.NonSerialized] private bool _activated = false;
    [System.NonSerialized] private Data _data;
    [System.NonSerialized] private Tween delayTween;

    private bool Enabled
    {
        set
        {
            _activated = value;
            if (value)
            {
                zonePs.Play();
                zonePs.transform.DOKill();
                zonePs.transform.DOScale(targetShowScale, 0.35f).SetEase(Ease.OutBack);
                
                frontLineMR.SetGradient(0.0f, 1.0f, 0.5f, GameManager.MPB_FRONT, "_BaseColor", Const.THIS.frontLineGradient);
            }
            else
            {
                frontLineMR.SetGradient(1.0f, 0.0f, 0.5f, GameManager.MPB_FRONT, "_BaseColor", Const.THIS.frontLineGradient);

                zonePs.transform.DOKill();
                zonePs.transform.DOScale(targetHideScale, 0.35f).SetEase(Ease.InBack).onComplete += () =>
                {
                    zonePs.Stop();
                };
            }
        }
        get => _activated;
    }
    
    public Data _Data
    {
        set
        {
            _data = value;
            if (_data.CanProtect)
            {
                StartProtection();
            }
        }
        get => _data;
    }
    
    public void AddShield(int amount, float duration)
    {
        _Data.amount += amount;
        _Data.AddTime(duration);
        _Data = _data;
    }
    public int ConsumeShield(int amount)
    {
        if (!_Data.CanProtect)
        {
            return -amount;
        }

        _Data.amount -= amount;

        int remaining = _Data.amount;
        _Data.amount = Mathf.Clamp(_Data.amount, 0, int.MaxValue);
        return remaining;
    }
    
    private void StartProtection()
    {
        if (!Enabled)
        {
            Enabled = true;
        }
        
        StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, _Data.amount, _Data.Percent, true, true);

        TickManager.THIS.AddTickable(this);
    }
    

    public float TickInterval { get => StatDisplayArranger.UpdateInterval;}

    public void OnTick()
    {
        StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, _Data.amount, _Data.Percent, false, false);

        if (_Data.CanProtect)
        {
            return;
        }

        _Data.amount = 0;
        
        Enabled = false;
            
        TickManager.THIS.RemoveTickable(this);
        StatDisplayArranger.THIS.Hide(StatDisplay.Type.Shield);
    }

    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public long startTick;
        [SerializeField] public long endTick;
        [SerializeField] public int amount;
        [System.NonSerialized] private long _difTicks;

        public Data()
        {
            
        }
        public Data(long startTick, long endTick, int amount)
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.amount = amount;

            this._difTicks = this.endTick - this.startTick;
        }

        public float Percent
        {
            get
            {
                long nowTick = DateTime.Now.Ticks;
                long ticks2Go = endTick - nowTick;
                return ticks2Go / (float)_difTicks;
            }
        }
        public bool CanProtect => AvailableByTime && amount > 0;
        public bool AvailableByTime => DateTime.Now.Ticks < endTick;

        public void AddTime(float duration)
        {
            startTick = DateTime.Now.Ticks;
            if (!AvailableByTime)
            {
                endTick = DateTime.Now.Ticks;
            }
            endTick += TimeSpan.FromSeconds(duration).Ticks;
            this._difTicks = this.endTick - this.startTick;
        }
        public Data(Shield.Data data)
        {
            this.startTick = data.startTick;
            this.endTick = data.endTick;
            this.amount = data.amount;
            this._difTicks = this.endTick - this.startTick;
        }

        public object Clone()
        {
            return new Shield.Data(this);
        }
    }
}
