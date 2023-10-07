using System;
using Internal.Core;
using UnityEngine;

public class IAPManager : Singleton<IAPManager>
{

    public const string RemoveAdBreak = "com.iwi.combatris.removeadbreak";


    public void Initialize()
    {
        
    }

    public void Purchase(PurchaseType purchaseType)
    {
        switch (purchaseType)
        {
            case PurchaseType.RemoveAdBreak:
                Debug.LogWarning(RemoveAdBreak);
                break;
        }
    }


    public enum PurchaseType
    {
        RemoveAdBreak,
    }
}
