using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class MarkedProgress : MonoBehaviour
{
    [Range(0.0f, 1.0f)] [SerializeField] private float progress;
    [SerializeField] private Vector2 range;
    [SerializeField] private Image fill;
    [SerializeField] private Transform startMark;
    [SerializeField] private Transform endMark;
    [SerializeField] private Transform currentMark;
    [System.NonSerialized] private Tween _animationTween;

    public float _Progress
    {
        set
        {
            progress = value;

            float remappedPercent = progress.Remap(0.0f, 1.0f, range.x, range.y);
            
            currentMark.position = Vector3.Lerp(startMark.position, endMark.position, remappedPercent);
            fill.fillAmount = remappedPercent;
        }
        get => progress;
    }
    
    public void ProgressAnimated(float value, float delay = 0.0f, Ease ease = Ease.Linear, System.Action<float> OnUpdate = null, System.Action OnEnd = null)
    {
        float duration = (value - _Progress) * 0.75f;
        float current = 0.0f;
        _animationTween?.Kill();
        _animationTween = DOTween.To((x) => current = x, _Progress, value, duration).SetEase(ease).SetDelay(delay).SetUpdate(true);
        _animationTween.onUpdate = () =>
        {
            _Progress = current;
            OnUpdate?.Invoke(current);
        };
        _animationTween.onComplete = () =>
        {
            OnEnd?.Invoke();
        };
    }
    
    private void OnValidate()
    {
        _Progress = progress;
    }

}