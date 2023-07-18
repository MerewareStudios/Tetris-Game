using System;
using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using TMPro;
using UnityEngine;

public class AdBreakScreen : Singleton<AdBreakScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI buttonText;
    [System.NonSerialized] private int _skipDuration;
    [System.NonSerialized] private System.Action _onShowAd;
    [System.NonSerialized] private System.Action _onSkip;
    [System.NonSerialized] private OnGetState _onCanSkip;
    [System.NonSerialized] private Coroutine _timerRoutine;
    
    public delegate bool OnGetState();
    
    public static void Set(int skipDuration, System.Action onShowAd, System.Action onSkip, OnGetState onCanSkip)
    {
        AdBreakScreen.THIS._skipDuration = skipDuration;
        AdBreakScreen.THIS._onShowAd = onShowAd;
        AdBreakScreen.THIS._onSkip = onSkip;
        AdBreakScreen.THIS._onCanSkip = onCanSkip;
    }

    private void StartTimer()
    {
        _timerRoutine = StartCoroutine(TimerRoutine());

        IEnumerator TimerRoutine()
        {
            Debug.LogWarning(_skipDuration);

            for (int i = _skipDuration; i > 0; i--)
            {
                buttonText.text = "YES(" + i + ")";
                yield return new WaitForSecondsRealtime(1.0f);
            }
            _onShowAd?.Invoke();
            Hide();
        }
    }

    private void StopTimer()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }
    }

    public static void Show()
    {
        AdBreakScreen.THIS.canvas.enabled = true;
        AdBreakScreen.THIS.StartTimer();
    }
    public void Hide()
    {
        canvas.enabled = false;
    }

    public void OnClick_Skip()
    {
        if (_onCanSkip.Invoke())
        {
            _onSkip?.Invoke();
            StopTimer();
            Hide();
        }
    }
}
