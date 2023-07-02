using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MoneyTransactor : Transactor<MoneyTransactor, int>
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public RectTransform IconPivot;
    [SerializeField] private RectTransform animationPivot;
    [SerializeField] private RectTransform scalePivot;

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
        }
    }

    public bool Transaction(int amount)
    {
        if (amount < 0 && base.TransactionData.value < -amount)
        {
            Punch(-0.15f);
            return false;
        }
        Punch(0.15f * Mathf.Sign(amount));
        Amount += amount;
        return true;
    }

    private void Punch(float amount)
    {
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOPunchScale(Vector3.one * amount, 0.35f, 1).SetUpdate(true);
    }
    
    public void Scale(float amount)
    {
        scalePivot.DOKill();
        scalePivot.DOScale(Vector3.one * amount, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
    }
}
