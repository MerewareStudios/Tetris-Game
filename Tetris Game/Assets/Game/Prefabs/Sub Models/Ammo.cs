using DG.Tweening;
using Game;
using TMPro;
using UnityEngine;

public class Ammo : SubModel
{
    [SerializeField] private TextMeshPro amountText;
    [System.NonSerialized] private bool _available = false;

    public override void OnConstruct(Pool poolType, Transform customParent, int extra)
    {
        base.OnConstruct(poolType, customParent, extra);
        MarkAvailable(false);
    }

    public override bool IsAvailable()
    {
        return _available;
    }

    public override void MarkAvailable(bool state)
    {
        this._available = state;
    }

    public override void OnExtraValueChanged(int value)
    {
        base.OnExtraValueChanged(value);
        // _amount = value;
        amountText.text = base.ExternalValue.ToString();
        BaseColor = (value == Board.THIS.StackLimit) ? Const.THIS.mergerMaxColor : Const.THIS.mergerColor;
    }

    public override int GetExtra()
    {
        return ExternalValue;
    }
}
