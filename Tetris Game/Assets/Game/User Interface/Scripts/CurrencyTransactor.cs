using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class CurrencyTransactor : Transactor<CurrencyTransactor, int>
{
    [SerializeField] private Const.CurrencyType currencyType;
    [SerializeField] public CurrencyDisplay currencyDisplay;
    [SerializeField] private RectTransform anchorPivot;
    [SerializeField] private RectTransform scalePivot;
    [SerializeField] private Vector3 defaultAnchor;
    [SerializeField] private Vector3 scaledAnchor;
    [System.NonSerialized] private float _targetScale = 1.0f;

    public override void Set(ref User.TransactionData<int> transactionData)
    {
        this.TransactionData = transactionData;
        Amount = transactionData.value;
    }

    public bool Active
    {
        set => anchorPivot.gameObject.SetActive(value);
    }
    
    public int Amount
    {
        get =>  base.TransactionData.value;
        set
        {
            base.TransactionData.value = value;
            currencyDisplay.Display(currencyType, value);
        }
    }
    
    public Const.Currency Currency => new Const.Currency(currencyType, base.TransactionData.value);

    public bool Transaction(int amount)
    {
        if (amount < 0 && Amount < amount.Abs())
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
        Active = true;
        scalePivot.DOKill();
        scalePivot.localScale = Vector3.one * _targetScale;
        scalePivot.DOPunchScale(Vector3.one * amount, 0.35f, 1).SetUpdate(true);
    }
    
    public void Scale(float amount, bool useScaledAnchor)
    {
        _targetScale = amount;
        
        // scalePivot.DOKill();
        // scalePivot.DOScale(Vector3.one * amount, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
        anchorPivot.DOKill();
        anchorPivot.DOAnchorPos(useScaledAnchor ? scaledAnchor : defaultAnchor, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
    }
}
