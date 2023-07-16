using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseOption : MonoBehaviour
{
    [SerializeField] private CurrenyButton purchaseButton;
    [SerializeField] private RectTransform animationPivot;
    [SerializeField] private CurrencyDisplay priceCurrencyDisplay;
    [SerializeField] private TextMeshProUGUI detailedInfoText;
    [SerializeField] private Image frameOutline;
    [TextArea] [SerializeField] private string detailedInfo;
    [TextArea] [SerializeField] private string purchaseInfo;
    [System.NonSerialized] private Sequence _colorSequence;

    public PurchaseOption SetPurchase(Const.Currency currency, bool able2Purchase)
    {
        purchaseButton.SetAvailable(able2Purchase);
        priceCurrencyDisplay.Display(currency);
        return this;
    }
    public PurchaseOption SetDetailedInfo(int gain)
    {
        detailedInfoText.text = string.Format(detailedInfo, gain);
        return this;
    }
    public string GetPurchaseInfo(int gain)
    {
        return string.Format(purchaseInfo, gain);
    }

    public void PunchColor(Color punchColor, Color defaultColor)
    {
        _colorSequence?.Kill();
        Tween punchColorTween = frameOutline.DOColor(punchColor, 0.1f).SetEase(Ease.Linear);
        Tween defaultColorTween = frameOutline.DOColor(defaultColor, 0.1f).SetEase(Ease.Linear).SetDelay(0.5f);

        _colorSequence = DOTween.Sequence().SetUpdate(true);
        _colorSequence.Append(punchColorTween);
        _colorSequence.Append(defaultColorTween);
    }
    public void Punch(Vector2 anchor)
    {
        animationPivot.DOKill();
        animationPivot.anchoredPosition = Vector2.zero;
        animationPivot.DOPunchAnchorPos(anchor, 0.25f).SetUpdate(true);
    } 
}
