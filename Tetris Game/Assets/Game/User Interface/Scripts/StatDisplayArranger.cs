using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Internal.Core;
using UnityEngine;

public class StatDisplayArranger : Singleton<StatDisplayArranger>
{
    [SerializeField] private List<StatDisplay> _statDisplays;
    [System.NonSerialized] public const float UpdateInterval = 0.05f;

    public void Show(StatDisplay.Type statType, int value, float timePercent = 1.0f, bool punch = false, bool setFront = false)
    {
        _statDisplays[(int)statType].Show(value, timePercent, punch, setFront);
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
