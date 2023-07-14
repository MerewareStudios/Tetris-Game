using System;
using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;

public class Wallet : Singleton<Wallet>
{
    [Header("Transactors")]
    [SerializeField] public CurrencyTransactor coin;
    [SerializeField] public CurrencyTransactor gem;

    public static CurrencyTransactor COIN => Wallet.THIS.coin;
    public static CurrencyTransactor GEM => Wallet.THIS.gem;
    
    public static Vector3 CoinIconPosition => Wallet.THIS.coin.currencyDisplay.iconPivot.position;

    public static bool HasFunds(Const.PurchaseType purchaseType, int amount)
    {
        CurrencyTransactor currencyTransactor = null;
        switch (purchaseType)
        {
            case Const.PurchaseType.Coin:
                currencyTransactor = Wallet.COIN;
                break;
            case Const.PurchaseType.Gem:
                currencyTransactor = Wallet.GEM;
                break;
            case Const.PurchaseType.Ad:
                currencyTransactor = null;
                break;
        }

        if (currencyTransactor)
        {
            return currencyTransactor.Amount >= amount;
        }

        return true;
    }
    
    public static bool Transaction(Const.PurchaseType purchaseType, int amount)
    {
        CurrencyTransactor currencyTransactor = null;
        switch (purchaseType)
        {
            case Const.PurchaseType.Coin:
                currencyTransactor = Wallet.COIN;
                break;
            case Const.PurchaseType.Gem:
                currencyTransactor = Wallet.GEM;
                break;
            case Const.PurchaseType.Ad:
                currencyTransactor = null;
                break;
        }

        return !currencyTransactor || currencyTransactor.Transaction(amount);
    }
}
