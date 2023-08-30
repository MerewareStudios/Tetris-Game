using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public RectTransform iconPivot;
    [System.NonSerialized] private Const.CurrencyType currencyType;

    public void Display(Const.Currency overridenCurrency)
    {
        text.text = overridenCurrency.type.ToTMProKey() + " " + overridenCurrency.amount;
        UpdateVisual(overridenCurrency.type);
    }
    public void Display(Const.CurrencyType type, int amount)
    {
        text.text = type.ToTMProKey() + " " + amount;
        UpdateVisual(type);
    }
    public void Display(Const.CurrencyType type)
    {
        text.text = type.ToTMProKey();
        UpdateVisual(type);
    }
    private void UpdateVisual(Const.CurrencyType overridenCurrencyType)
    {
        if (this.currencyType.Equals(overridenCurrencyType)) return;
        
        this.currencyType = overridenCurrencyType;
        Const.THIS.SetCurrencyColor(text, overridenCurrencyType);
    }
    public void Display(string str, bool buttonActive)
    {
        text.text = str;
    }
    public void Set(string str)
    {
        text.text = str;
    }
}
