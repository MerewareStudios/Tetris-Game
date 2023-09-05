using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class CustomPower : Lazyingleton<CustomPower>
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform buttonRect;
    [SerializeField] private RectTransform buttonPunchPivot;
    [SerializeField] public Transform clickTarget;
    [SerializeField] private Button button;
    [SerializeField] private Animator _animator;

    public bool Available { get; private set; }

    public void Show(bool enableAnimator = true)
    {
        this.gameObject.SetActive(true);
        _canvas.enabled = true;
        _animator.enabled = false;

        Available = true;

        buttonRect.DOKill();
        buttonRect.localScale = Vector3.zero;
        buttonRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).onComplete = () =>
        {
            _animator.enabled = enableAnimator;
            button.targetGraphic.raycastTarget = true;
        };
    }

    public void Highlight()
    {
        Board.THIS.ShowTicketMergePlaces();
    }
    
    public void HighlightPunch()
    {
        Board.THIS.ShowTicketMergePlaces();
        buttonPunchPivot.DOKill();
        buttonPunchPivot.localScale = Vector3.one;
        buttonPunchPivot.DOPunchScale(Vector3.one * 0.15f, 0.25f, 1);
    }

    public void HideAnimated()
    {
        buttonRect.DOKill();
        buttonRect.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).onComplete = () =>
        {
            this.gameObject.SetActive(false);
            _canvas.enabled = false;
            
            Available = false;
        };
    }

    public void OnClick()
    {
        button.targetGraphic.raycastTarget = false;
        _animator.enabled = false;

        if (ONBOARDING.LEARN_TICKET_MERGE.IsNotComplete())
        {
            ONBOARDING.LEARN_TICKET_MERGE.SetComplete();
            UIManager.THIS.speechBubble.Hide();
            Onboarding.StopRoutine();
            Onboarding.HideFinger();
        }
        // if (Wallet.Consume(Const.Currency.OneAd))
        {
            OnPowerUsed();
        }
        // else
        {
            
        }
    }

    private void OnPowerUsed()
    {
        Map.THIS.ForceMerge(0);
        Board.THIS.ShowTicketMergePlaces();

        HideAnimated();
    }
    
    

    public void Deconstruct()
    {
        button.targetGraphic.raycastTarget = false;
        _animator.enabled = false;

        HideAnimated();
    }
}
