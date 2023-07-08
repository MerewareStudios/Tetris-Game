using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ShopBar : Transactor<ShopBar, float>
{
    [SerializeField] public Image fill;
    [SerializeField] private RectTransform animationPivot;
    [SerializeField] private RectTransform prompt;
    [SerializeField] private UnityEvent OnClickAction;
    [FormerlySerializedAs("particleSystem")] [SerializeField] private ParticleSystem effectPS;
    [System.NonSerialized] private Tween fillTween;
    
    public override void Set(ref User.TransactionData<float> transactionData)
    {
        this.TransactionData = transactionData;
        Amount = transactionData.value;
        
        if (base.TransactionData.value >= 1.0f)
        {
            ShowPrompt();
        }
    }
    
    public float Amount
    {
        get => base.TransactionData.value;
        set
        {
            if (base.TransactionData.value >= 1.0f)
            {
                return;
            }
            
            base.TransactionData.value = Mathf.Clamp(value, 0.0f, 1.0f);
            
            PunchScale(0.3f);
            
            if (base.TransactionData.value >= 1.0f)
            {
                ShowPrompt();
            }
            
            Fill = base.TransactionData.value;
        }
    }
    private void PunchScale(float magnitude)
    {
        animationPivot.DOKill();
        animationPivot.localScale = Vector3.one;
        animationPivot.DOPunchScale(Vector3.one * magnitude, 0.25f, 1);
    }
    public float Fill
    {
        set
        {
            fillTween?.Kill();
            fillTween = fill.DOFillAmount(value, 0.25f).SetEase(Ease.OutCirc).SetUpdate(true);
        }
    }

    public void User_OnClick()
    {
        if (base.TransactionData.value < 1.0f)
        {
            return;
        }
        OnClickAction?.Invoke();
        
        effectPS.Stop();
        // ConsumeFill();
    }

    public void ConsumeFill()
    {
        HidePrompt();
        
        base.TransactionData.value = 0.0f;
        Fill = base.TransactionData.value;
    }
    
    public void ShowPrompt()
    {
        effectPS.Play();
        prompt.gameObject.SetActive(true);
        prompt.localScale = Vector3.one;
        prompt.DOKill();
        prompt.DOLocalMoveY(25.0f, 0.5f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetUpdate(true);
    }
    public void HidePrompt()
    {
        prompt.DOKill();
        prompt.DOScale(Vector3.zero, 0.25f).SetUpdate(true).SetEase(Ease.InBack).onComplete += () =>
        {
            prompt.gameObject.SetActive(false);
        };
    }
}
