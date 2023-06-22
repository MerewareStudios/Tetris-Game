using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class Shield : MonoBehaviour
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
            if (_data.timeRemaining > 0.0f)
            {
                StartProtection();
            }
            
        }
        get => _data;
    }
    
    public float _Protection
    {
        set
        {
            _data.timeRemaining = value;
            if (_data.timeRemaining > 0.0f)
            {
                StartProtection();
            }
            
        }
        get => _data.timeRemaining;
    }

    private void StartProtection()
    {
        if (!Enabled)
        {
            Enabled = true;
        }
        
        StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, Mathf.CeilToInt(_data.timeRemaining), true, true);

        delayTween?.Kill();
        delayTween = DOVirtual.DelayedCall(_data.timeRemaining, () =>
        {
            Enabled = false;
            _Data.timeRemaining = 0.0f;
            StatDisplayArranger.THIS.Hide(StatDisplay.Type.Shield);
        });
        delayTween.onUpdate += () =>
        {
            _Data.timeRemaining -= Time.deltaTime;
            StatDisplayArranger.THIS.Show(StatDisplay.Type.Shield, Mathf.CeilToInt(_data.timeRemaining));
        };
    }

    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public float timeRemaining = 0.0f;

        public Data()
        {
            this.timeRemaining = 0.5f;
        }
        public Data(float timeRemaining)
        {
            this.timeRemaining = timeRemaining;
        }
        public Data(Shield.Data data)
        {
            this.timeRemaining = data.timeRemaining;
        }

        public object Clone()
        {
            return new Shield.Data(this);
        }
    }
}
