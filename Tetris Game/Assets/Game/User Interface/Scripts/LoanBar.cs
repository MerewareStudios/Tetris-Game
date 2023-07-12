using System;
using DG.Tweening;
using Game;
using Internal.Core;
using IWI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LoanBar : MonoBehaviour
{
    [SerializeField] private RectTransform scalePivot;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI currencyText;
    [System.NonSerialized] private State _currentState = State.CLOSE;
    [System.NonSerialized] private float _showProtectionStamp = 0.0f;
    [SerializeField] private float textUpdateUpdateInterval = 1.0f;
    [System.NonSerialized] private float _textUpdateTimeStamp = 0.0f;
    
    public bool Show()
    {
        if (Time.time - _showProtectionStamp < Const.THIS.loanBarProtectionInterval)
        {
            return false;
        }
        if (_currentState.Equals(State.OPEN))
        {
            return false;
        }

        _currentState = State.OPEN;
        
        button.image.raycastTarget = false;
        gameObject.SetActive(true); 

        scalePivot.DOKill();
        scalePivot.localScale = Vector3.zero;
        scalePivot.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack)
                .onComplete +=
            () =>
            {
                button.image.raycastTarget = true;
            };
        
        return true;
    }

    private void Update()
    {
        if (Time.time - _textUpdateTimeStamp > textUpdateUpdateInterval)
        {
            int secondsLeft = AdManager.THIS._Data.LoanBarSecondsLeft;
            currencyText.text = secondsLeft > 0 ? secondsLeft.ToString() : Const.PurchaseType.Ad.ToTMProKey();

            _textUpdateTimeStamp = Time.time;
        }
    }

    public bool Hide()
    {
        if (_currentState.Equals(State.CLOSE))
        {
            return false;
        }

        _currentState = State.CLOSE;
        
        button.image.raycastTarget = false;

        scalePivot.DOKill();
        scalePivot.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack)
                .onComplete +=
            () =>
            {
                gameObject.SetActive(false); 
            };
        
        return true;
    }
    
    public void OnClick_Use()
    {
        if (!AdManager.THIS._Data.CanUseLoanBar)
        {
            return;
        }
        _showProtectionStamp = Time.time;
        
        Hide();
        Spawner.THIS.InterchangeBlock(Pool.Single_Block, Pawn.Usage.HorMerge);
        AdManager.THIS._Data.StampLoanBar();
    }

    public enum State
    {
        OPEN,
        CLOSE,
    }
}
