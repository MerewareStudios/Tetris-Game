using Game;
using TMPro;
using UnityEngine;

public class Ammo : SubModel
{
    [SerializeField] private TextMeshPro amountText;
    
    public override void OnExternalValueChanged(int value)
    {
        base.OnExternalValueChanged(value);
        amountText.text = base.ExternalValue.ToString();
        BaseColor = (value == Board.THIS.StackLimit) ? Const.THIS.mergerMaxColor : Const.THIS.mergerColor;
    }
}
