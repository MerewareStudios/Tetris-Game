using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardButton : MonoBehaviour
{
    [System.NonSerialized] private System.Action OnClickAction;
    [SerializeField] private Button ActionButton;
    [SerializeField] private Image piggyIcon;
    [SerializeField] private TextMeshProUGUI iconText;
    [SerializeField] private TextMeshProUGUI amountText;
    
    public RewardButton OnClick(System.Action OnClick)
    {
        this.OnClickAction = OnClick;
        return this;
    }

    public void OnClick()
    {
        OnClickAction?.Invoke();
    }

    // public void ShowReward(PiggyMenu.PiggyReward piggyReward)
    // {
    //     ActionButton.image.raycastTarget = false;
    //
    //     piggyIcon.rectTransform.DOKill();
    //     piggyIcon.rectTransform.localScale = Vector3.one;
    //     piggyIcon.rectTransform.localRotation = Quaternion.identity;
    //     piggyIcon.DOColor(Const.THIS.piggyExplodeColor, 0.25f).SetUpdate(true);
    //     piggyIcon.rectTransform.DOScale(Vector3.one * 1.25f, 0.25f).SetEase(Const.THIS.piggyExplodeEase).SetUpdate(true).onComplete += () =>
    //     {
    //         // piggyReward.GiveReward();
    //         
    //         Particle.Piggy_Break_Ps.Emit(100, piggyIcon.rectTransform.position);
    //     
    //         piggyIcon.enabled = false;
    //
    //         iconText.text = piggyReward.type.ToString().ToTMProKey();
    //         amountText.text = piggyReward.amount.ToString();
    //         
    //         iconText.gameObject.SetActive(true);
    //         iconText.rectTransform.DOKill();
    //         iconText.rectTransform.localScale = Vector3.zero;
    //         iconText.rectTransform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true);
    //         
    //         amountText.gameObject.SetActive(true);
    //         amountText.rectTransform.DOKill();
    //         amountText.rectTransform.localScale = Vector3.zero;
    //         amountText.rectTransform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true);
    //     };
    // }
    public RewardButton Show(Vector3 start, Vector3 end, float delay)
    {
        this.gameObject.SetActive(true);
        
        iconText.gameObject.SetActive(false);
        amountText.gameObject.SetActive(false);
        
        piggyIcon.rectTransform.localScale = Vector3.one;
        piggyIcon.color = Color.white;
        piggyIcon.enabled = true;

        RectTransform thisTransform = transform as RectTransform;
        thisTransform.DOKill();
        thisTransform.position = start;
        thisTransform.localScale = Vector3.zero;
        thisTransform.DOLocalMove(end, 0.35f).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true);
        thisTransform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetDelay(delay).SetUpdate(true).onComplete +=
            () =>
            {
                ActionButton.image.raycastTarget = true;
            };
        return this;
    }
}
