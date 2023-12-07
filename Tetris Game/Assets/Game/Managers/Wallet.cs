using UnityEngine;

public static class Wallet
{
    public static CurrencyTransactor COIN => UIManager.THIS.coin;
    public static CurrencyTransactor PIGGY => UIManager.THIS.gem;
    public static CurrencyTransactor TICKET => UIManager.THIS.ticket;

    public static CurrencyTransactor[] CurrencyTransactors;

    public static Vector3 IconPosition(this Const.CurrencyType currencyType)
    {
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currencyType];

        return currencyTransactor.currencyDisplay.iconPivot.position;
    }
    
    public static bool HasFunds(Const.Currency currency)
    {
        if (!Exists(currency))
        {
            return true;
        }
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currency.type];

        if (currencyTransactor)
        {
            return currencyTransactor.Amount >= currency.amount;
        }

        return true;
    }

    public static bool Exists(Const.Currency currency)
    {
        return (int)currency.type < Wallet.CurrencyTransactors.Length;
    }
    
    public static bool Transaction(Const.Currency currency)
    {
        if (!Exists(currency))
        {
            return true;
        }
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currency.type];

        return !currencyTransactor || currencyTransactor.Add(currency.amount);
    }
    public static bool Consume(Const.Currency currency)
    {
        if (!Exists(currency))
        {
            return true;
        }
        CurrencyTransactor currencyTransactor = Wallet.CurrencyTransactors[(int)currency.type];

        return !currencyTransactor || currencyTransactor.Consume(currency.amount);
    }
    
    public static void ScaleTransactors(float scale, bool distance = false)
    {
        COIN.Scale(scale, distance);
        PIGGY.Scale(scale, distance);
        TICKET.Scale(scale, distance);
    }
}
