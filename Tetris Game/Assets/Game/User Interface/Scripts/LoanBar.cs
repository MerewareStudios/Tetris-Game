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
    [SerializeField] private Button button;
    [SerializeField] private RectTransform scalePivot;
    [SerializeField] private CurrencyDisplay priceText;
    [SerializeField] private Const.Currency currency;
    [System.NonSerialized] private float _timeAvaliable = 0.0f;

    private int TimeLeft => (int)(_timeAvaliable - Time.time); 
    
    public void MakeAvailable()
    {
        if (this.gameObject.activeSelf)
        {
            return;
        }
        if (TimeLeft > 0)
        {
            return;
        }
        Show();
        priceText.Display(currency);
    }
    
    public void MakeUnavailable(float timeOffset)
    {
        _timeAvaliable = Time.time + timeOffset;
        gameObject.SetActive(false);
    }

    private void Show()
    {
        gameObject.SetActive(true);
        button.image.raycastTarget = false;

        scalePivot.DOKill();
        scalePivot.localScale = Vector3.zero;
        scalePivot.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack)
                .onComplete +=
            () =>
            {
                button.image.raycastTarget = true;
            };
    }
    
    public void OnClick_Use()
    {
        if (TimeLeft > 0)
        {
            return;
        }
        MakeUnavailable(Const.THIS.adSettings.loanBarProtectionInterval);
        Spawner.THIS.InterchangeBlock(Pool.Single_Block, Pawn.Usage.HorMerge);
    }
}
