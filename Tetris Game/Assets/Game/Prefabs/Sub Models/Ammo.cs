using Game;
using TMPro;
using UnityEngine;

public class Ammo : SubModel
{
    [SerializeField] private TextMeshPro amountText;
    // [System.NonSerialized] private int _amount;
    
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
