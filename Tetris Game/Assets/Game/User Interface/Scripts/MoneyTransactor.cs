using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyTransactor : Transactor<MoneyTransactor, int>
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private RectTransform animationPivot;

    public override void Set(ref User.TransactionData<int> transactionData)
    {
        this.TransactionData = transactionData;
        Amount = transactionData.value;
    }
    
    public int Amount
    {
        get => base.TransactionData.value;
        set
        {
            base.TransactionData.value = value;

            text.text = value.CoinAmount();
            
            Punch(0.15f);
        }
    }

    public bool Transaction(int amount)
    {
        if (base.TransactionData.value < amount)
        {
            Punch(-0.15f);
            return false;
        }

        Amount -= amount;
        return true;
    }

    private void Punch(float amount)
    {
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOPunchScale(Vector3.one * amount, 0.35f, 1).SetUpdate(true);
    }
}
