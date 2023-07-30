using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
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
    [System.NonSerialized] private float _lastTimeTicked;

    private bool Enabled
    {
        set
        {
            if (_activated == value)
            {
                return;
            }
            _activated = value;
            if (value)
            {
                zonePs.Play();
                zonePs.transform.DOKill();
                zonePs.transform.DOScale(targetShowScale, 0.35f).SetEase(Ease.OutBack);
                
                frontLineMR.SetGradient(0.0f, 1.0f, 0.5f, GameManager.MPB_FRONT, GameManager.BaseColor, Const.THIS.frontLineGradient);
            }
            else
            {
                frontLineMR.SetGradient(1.0f, 0.0f, 0.5f, GameManager.MPB_FRONT, GameManager.BaseColor, Const.THIS.frontLineGradient);

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
                ResumeProtection();
            }
        }
        get => _data;
    }

    public void AddShield(int amount)
    {
        _Data.amount += amount;
        _Data.SetTime(Const.THIS.shieldMaxDuration);
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
    
    private void DisplayProtection()
    {
        StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, _Data.amount, _Data.Percent, true, true);
        Enabled = true;
    }
    public void ResumeProtection()
    {
        _lastTimeTicked = Time.time;
        if (_data.CanProtect)
        {
            DisplayProtection();
            if (!TickManager.THIS.IsTicking(this))
            {
                TickManager.THIS.AddTickable(this);
            }
        }
    }
    public void PauseProtection()
    {
        TickManager.THIS.RemoveTickable(this);
    }

    public float TickInterval { get => StatDisplayArranger.UpdateInterval;}

    public void OnTick()
    {
        _Data.ConsumeTime(Time.time - _lastTimeTicked);
        _lastTimeTicked = Time.time;
        
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
        [SerializeField] public float maxTime = 0.0f;
        [SerializeField] public float timeLeft = 0.0f;
        [SerializeField] public int amount;

        public Data()
        {
            
        }
        public Data(float timeLeft, float maxTime, int amount)
        {
            this.timeLeft = timeLeft;
            this.maxTime = maxTime;
            this.amount = amount;
        }

        public float Percent => timeLeft / maxTime;
        public bool CanProtect => AvailableByTime && amount > 0;
        public bool AvailableByTime => timeLeft > 0.0f;

        public void AddTime(float duration)
        {
            timeLeft += duration;
            maxTime = timeLeft;
        }
        public void SetTime(float duration)
        {
            timeLeft = duration;
            maxTime = timeLeft;
        }
        public void ConsumeTime(float duration)
        {
            timeLeft = Mathf.Clamp(timeLeft - duration, 0.0f, float.MaxValue);
        }
        public Data(Shield.Data data)
        {
            this.timeLeft = data.timeLeft;
            this.maxTime = data.maxTime;
            this.amount = data.amount;
        }

        public object Clone()
        {
            return new Shield.Data(this);
        }
    }
}
