
using TMPro;
using UnityEngine;

public class UnpackedAmmo : SubModel
{
    [SerializeField] private TextMeshPro amountText;

    public override void OnExternalValueChanged(int value)
    {
        base.OnExternalValueChanged(value);
        amountText.text = base.ExternalValue.ToString();
    }
}
