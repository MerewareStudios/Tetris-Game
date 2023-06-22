using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

public class StatDisplayArranger : Singleton<StatDisplayArranger>
{
    [SerializeField] private List<StatDisplay> _statDisplays;

    public void Show(StatDisplay.Type statType, int value, bool punch = false, bool setFront = false)
    {
        _statDisplays[(int)statType].Show(value, punch, setFront);
    }
    public void Hide(StatDisplay.Type statType)
    {
        _statDisplays[(int)statType].Hide();
    }

    public Vector3 ScreenPosition(StatDisplay.Type statType)
    {
        return _statDisplays[(int)statType].animationPivot.position;

    }
}
