using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.Events;

public class CurrencyTransactor : Transactor<CurrencyTransactor, int>
{
    [SerializeField] private Const.CurrencyType currencyType;
    [SerializeField] public CurrencyDisplay currencyDisplay;
    [SerializeField] private RectTransform anchorPivot;
    [SerializeField] private RectTransform scalePivot;
    [SerializeField] private Vector3 defaultAnchor;
    [SerializeField] private Vector3 scaledAnchor;
    [SerializeField] private UnityEvent onConsume;
    [SerializeField] private Audio audioOnConsume;
    [System.NonSerialized] private float _targetScale = 1.0f;

    public override void Set(ref User.TransactionData<int> transactionData)
    {
        this.TransactionData = transactionData;
        Amount = transactionData.value;
    }

    public bool Active
    {
        set
        {
            
#if CREATIVE
            switch (currencyType)
            {
                case Const.CurrencyType.Coin:
                    if (!Const.THIS.creativeSettings.coinEnabled)
                    {
                        anchorPivot.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case Const.CurrencyType.Gem:
                    if (!Const.THIS.creativeSettings.gemEnabled)
                    {
                        anchorPivot.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case Const.CurrencyType.Ticket:
                    if (!Const.THIS.creativeSettings.ticketEnabled)
                    {
                        anchorPivot.gameObject.SetActive(false);
                        return;
                    }
                    break;
            }
           
#endif
            
            anchorPivot.gameObject.SetActive(value);
        }
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

    public bool Add(int amount)
    {
        Punch(0.15f * Mathf.Sign(amount));
        Amount += amount;
        return true;
    }
    
    public bool Consume(int amount)
    {
        if (Amount < amount.Abs())
        {
            Punch(-0.15f);
            return false;
        }
        audioOnConsume.PlayOneShot();
        Punch(0.15f * Mathf.Sign(amount));
        Amount -= amount;
        onConsume?.Invoke();
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
        
        scalePivot.DOKill();
        scalePivot.DOScale(Vector3.one * amount, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
        anchorPivot.DOKill();
        anchorPivot.DOAnchorPos(useScaledAnchor ? scaledAnchor : defaultAnchor, 0.35f).SetUpdate(true).SetEase(Ease.OutSine);
    }
}
