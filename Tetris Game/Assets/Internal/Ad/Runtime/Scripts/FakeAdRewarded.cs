using Internal.Core;
using UnityEngine;

public class FakeAdRewarded : Lazyingleton<FakeAdRewarded>
{
    [SerializeField] private Canvas canvas;
    private static System.Action _onReward;
    private static System.Action _onSkip;
    
    public static void Show(System.Action onReward, System.Action onSkip = null)
    {
        FakeAdRewarded.THIS.canvas.enabled = true;
        _onReward = onReward;
        _onSkip = onSkip;
    }

    public void OnClick_OnReward()
    {
        _onReward?.Invoke();
        FakeAdRewarded.THIS.canvas.enabled = false;
    }
    public void OnClick_OnSkip()
    {
        _onSkip?.Invoke();
        FakeAdRewarded.THIS.canvas.enabled = false;
    }
}
