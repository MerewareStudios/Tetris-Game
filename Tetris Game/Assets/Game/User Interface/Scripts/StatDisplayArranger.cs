using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

public class StatDisplayArranger : Singleton<StatDisplayArranger>
{
    [SerializeField] private List<StatDisplay> _statDisplays;
    // [System.NonSerialized] public const float UpdateInterval = 0.05f;

    public void Show(StatDisplay.Type statType, int value, float timePercent = 1.0f, bool punch = false)
    {
        _statDisplays[(int)statType].Show(value, timePercent, punch);
    }
    public void UpdatePercent(StatDisplay.Type statType, float percent)
    {
        _statDisplays[(int)statType].UpdatePercent(percent);
    }
    public void UpdateAmount(StatDisplay.Type statType, int amount, float punch, bool markSpecial = false)
    {
        _statDisplays[(int)statType].UpdateAmount(amount, punch, markSpecial);
    }
    public void Hide(StatDisplay.Type statType)
    {
        _statDisplays[(int)statType].Hide();
    }
    public void HideImmediate(StatDisplay.Type statType)
    {
        _statDisplays[(int)statType].HideImmediate();
    }

    public Vector3 ScreenPosition(StatDisplay.Type statType)
    {
        return _statDisplays[(int)statType].animationPivot.position;

    }
}
