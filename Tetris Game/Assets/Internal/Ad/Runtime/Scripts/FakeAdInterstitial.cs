using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

public class FakeAdInterstitial : Singleton<FakeAdInterstitial>
{
    [SerializeField] private Canvas canvas;

    private static System.Action _onFinish;
    
    public static void Show(System.Action onFinish)
    {
        FakeAdInterstitial.THIS.canvas.enabled = true;
        _onFinish = onFinish;
    }

    public void OnClick_OnFinish()
    {
        _onFinish?.Invoke();
        FakeAdInterstitial.THIS.canvas.enabled = false;
    }
}
