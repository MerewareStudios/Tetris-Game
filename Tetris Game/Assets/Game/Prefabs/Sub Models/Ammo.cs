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
    
    public override void EmitExplodeEffect()
    {
        Particle.Debris.Emit(5, Position, base._color);
    }

    public override bool IsAvailable()
    {
        return _available;
    }

    public override void OnUse()
    {
        base.OnUse();
        
        ThisTransform.DOKill();

        if (ExternalValue > 0)
        {
            ThisTransform.localScale = Vector3.one;
            ThisTransform.DOPunchScale(Vector3.one * -0.4f, 0.3f, 1);
        }
        else
        {
            ThisTransform.DOScale(Vector3.zero, 0.175f).SetEase(Ease.InBack, 2.0f).onComplete = () =>
            {
                base.OnDeconstruct();
            };
        }
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
