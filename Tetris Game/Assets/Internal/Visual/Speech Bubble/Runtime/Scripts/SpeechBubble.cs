using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using TMPro;
using UnityEngine;

public class SpeechBubble : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;
    [System.NonSerialized] private Tween _tween;
    [System.NonSerialized] private Tween _delayTween;
    public TypewriterByCharacter textAnimatorPlayer;

    public void Speak(string txt, float delay = 0.0f, float? autoCloseDelay = null)
    {
        _canvas.enabled = true;
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
        
        if (autoCloseDelay != null)
        {
            _delayTween?.Kill();
            _delayTween = DOVirtual.DelayedCall((float)autoCloseDelay, Hide, false);
        }
    }
    
    public void Hide()
    {
        if (!_canvas.enabled)
        {
            return;
        }
        _delayTween?.Kill();

        _tween?.Kill();
        _tween = canvasGroup.DOFade(0.0f, 0.2f).SetEase(Ease.InOutSine);
        _tween.onComplete += () =>
        {
            this.gameObject.SetActive(false);
            _canvas.enabled = false;
        };
    }
}
