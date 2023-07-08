using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

public class Toast : Singleton<Toast>
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI text;
    [System.NonSerialized] private Sequence _sequence;

    void Awake()
    {
        _canvas.enabled = false;
        _canvasGroup.alpha = 0.0f;
    }

    public static void Show(string message, float duration)
    {
        Toast.THIS.ShowMessage(message, duration);
    }

    private void ShowMessage(string message, float duration)
    {
        _canvas.enabled = true;
        _canvasGroup.alpha = 0.0f;

        text.text = message;
        Tween showTween = _canvasGroup.DOFade(1.0f, 0.1f).SetEase(Ease.InSine).SetUpdate(true);
        Tween hideTween = _canvasGroup.DOFade(0.0f, 0.15f).SetEase(Ease.OutSine).SetDelay(duration).SetUpdate(true);
        
        _sequence?.Kill();
        _sequence = DOTween.Sequence().SetUpdate(true);
        _sequence.Append(showTween);
        _sequence.Join(hideTween);
        _sequence.onComplete = () => _canvas.enabled = false;
    }
}
