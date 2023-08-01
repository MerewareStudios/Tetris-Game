using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;
    [System.NonSerialized] private Tween _tween;
    [System.NonSerialized] private Tween _delayTween;
    public TypewriterByCharacter textAnimatorPlayer;

    public void Speak(string txt, float delay = 0.0f, float? closeDelay = null)
    {
        _delayTween?.Kill();
            
        this.gameObject.SetActive(true);
        canvasGroup.alpha = 0.0f;
        text.text = "";
        
        _tween?.Kill();
        _tween = canvasGroup.DOFade(1.0f, 0.2f).SetEase(Ease.InOutSine).SetDelay(delay);

        _tween.OnStart(() =>
        {
            textAnimatorPlayer.ShowText(txt);
        });
        
        if (closeDelay != null)
        {
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall((float)closeDelay, Hide);
        }
    }
    
    public void Hide()
    {
        _delayTween?.Kill();

        _tween?.Kill();
        _tween = canvasGroup.DOFade(0.0f, 0.2f).SetEase(Ease.InOutSine);
        _tween.onComplete += () =>
        {
            this.gameObject.SetActive(false);
        };
    }
}
