using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;
    [System.NonSerialized] private Tween _tween;
    
    public void Speak(string txt, float delay = 0.0f)
    {
        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0.0f;
        text.text = txt;
        
        _tween?.Kill();
        _tween = canvasGroup.DOFade(1.0f, 0.2f).SetEase(Ease.InOutSine).SetDelay(delay);
    }
    
    public void Hide()
    {
        _tween?.Kill();
        _tween = canvasGroup.DOFade(0.0f, 0.2f).SetEase(Ease.InOutSine);
        _tween.onComplete += () =>
        {
            this.gameObject.SetActive(false);
        };
    }
}
