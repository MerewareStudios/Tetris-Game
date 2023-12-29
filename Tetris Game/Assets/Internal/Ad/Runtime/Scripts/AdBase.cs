using System;
using Internal.Core;
using UnityEngine;

public abstract class AdBase<T> : Lazyingleton<T> where T : MonoBehaviour
{
    [System.NonSerialized] private bool _invokingForLoad = false;
    [System.NonSerialized] private int _retryAttempt;

    public virtual void LoadAd()
    {
        _invokingForLoad = true;
    }
    
    protected void ForwardInvoke()
    {
        if (_invokingForLoad)
        {
            CancelInvoke(nameof(LoadAd));
            LoadAd();
        }
    }
    protected void InvokeForLoad()
    {
        _retryAttempt++;
        Invoke(nameof(LoadAd), Mathf.Pow(2, Math.Min(6, _retryAttempt)));
        _invokingForLoad = true;
    }
    private void IncreaseAttempts()
    {
        _retryAttempt++;
    }
    protected void ResetAttempts()
    {
        _retryAttempt = 0;
    }
}
