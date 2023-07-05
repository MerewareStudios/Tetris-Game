using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

public class Transactor<T, TYPE> : MonoBehaviour where T : MonoBehaviour
{
    [System.NonSerialized] protected User.TransactionData<TYPE> TransactionData;

    public virtual void Set(ref User.TransactionData<TYPE> transactionData)
    {
        this.TransactionData = transactionData;
    }
    
}
