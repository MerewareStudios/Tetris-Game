using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

public class StatDisplayArranger : Lazyingleton<StatDisplayArranger>
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private List<StatDisplay> _statDisplays;
    [SerializeField] private RectTransform pivot;

    public Vector3 World2ScreenPosition
    {
        set
        {
            Vector2 localPoint = CameraManager.THIS.gameCamera.WorldToScreenPoint(value);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, localPoint, _canvas.worldCamera, out Vector2 local);
            pivot.localPosition = local;
        }
    }

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
