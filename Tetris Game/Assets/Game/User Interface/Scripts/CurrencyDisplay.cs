using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public RectTransform iconPivot;
    [SerializeField] private Const.PurchaseType purchaseType;

    public void Display(int amount)
    {
        text.text = purchaseType.ToTMProKey() + amount;
    }
    
    public void Display(Const.PurchaseType overridenPurchaseType, int amount)
    {
        text.text = overridenPurchaseType.ToTMProKey() + amount;
    }
}
