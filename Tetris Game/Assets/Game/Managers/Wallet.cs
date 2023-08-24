using System;
using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using RootMotion.Demos;
using UnityEngine;
using UnityEngine.Serialization;

public static class Wallet
{
    public static CurrencyTransactor COIN => UIManager.THIS.coin;
    public static CurrencyTransactor GEM => UIManager.THIS.gem;
    public static CurrencyTransactor AD => UIManager.THIS.ad;

    public static CurrencyTransactor[] CurrencyTransactors;
    
    public static Vector3 IconPosition(this Const.CurrencyType currencyType)
    {
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currencyType];

        return currencyTransactor.currencyDisplay.iconPivot.position;
    }
    
    public static bool HasFunds(Const.Currency currency)
    {
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currency.type];

        if (currencyTransactor)
        {
            return currencyTransactor.Amount >= currency.amount;
        }

        return true;
    }
    
    public static bool Transaction(Const.Currency currency)
    {
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currency.type];

        return !currencyTransactor || currencyTransactor.Transaction(currency.amount);
    }
    public static bool Consume(Const.Currency currency)
    {
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currency.type];

        return !currencyTransactor || currencyTransactor.Transaction(-currency.amount);
    }
    
    public static void ScaleTransactors(float scale, bool distance = false)
    {
        COIN.Scale(scale, distance);
        GEM.Scale(scale, distance);
        AD.Scale(scale, distance);
    }
    public static void HideTransactors()
    {
        
    }
    public static void ShowTransactors()
    {
        
    }
}
