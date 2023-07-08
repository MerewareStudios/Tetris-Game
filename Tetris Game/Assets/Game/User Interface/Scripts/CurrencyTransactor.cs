using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class CurrencyTransactor : Transactor<CurrencyTransactor, int>
{
    [SerializeField] private Const.PurchaseType purchase;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] public RectTransform iconPivot;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private Vector3 defaultAnchor;
    [SerializeField] private Vector3 scaledAnchor;
    [System.NonSerialized] private float _targetScale = 1.0f;

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
            text.text = purchase.ToTMProKey() + value;
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
        pivot.DOKill();
        pivot.localScale = Vector3.one * _targetScale;
        pivot.DOPunchScale(Vector3.one * amount, 0.35f, 1).SetUpdate(true);
    }
    
    public void Scale(float amount, bool useScaledAnchor)
    {
        _targetScale = amount;
        
        pivot.DOKill();
        pivot.DOScale(Vector3.one * amount, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
        pivot.DOAnchorPos(useScaledAnchor ? scaledAnchor : defaultAnchor, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
    }
}
