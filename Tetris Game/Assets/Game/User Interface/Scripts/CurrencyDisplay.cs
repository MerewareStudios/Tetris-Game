using Internal.Core;
using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public RectTransform iconPivot;
    [System.NonSerialized] private Const.CurrencyType currencyType;

    public static string GetCurrencyString(Const.Currency currency)
    {
        return currency.type.ToTMProKey() + " " + currency.amount;
    }
    public void Display(string currency)
    {
        text.text = currency;
        UpdateVisual(Const.CurrencyType.Local);
    }
    public void Display(Const.Currency overridenCurrency)
    {
        text.text = overridenCurrency.type.ToTMProKey() + " " + overridenCurrency.amount;
        UpdateVisual(overridenCurrency.type);
    }
    public void Display(Const.Currency overridenCurrency, int max)
    {
        text.text = overridenCurrency.type.ToTMProKey() + " " + overridenCurrency.amount + "/" + max;
        UpdateVisual(overridenCurrency.type);
    }
    public void Display(int current, int max)
    {
        text.text = currencyType.ToTMProKey() + " " + current + "/" + max;
        UpdateVisual(currencyType);
    }
    public void DisplayRealMoneyWithFraction(Const.Currency overridenCurrency)
    {
        float amount = overridenCurrency.amount;
        amount -= 0.01f;
        text.text = "$" + amount.ToString("F2");
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
