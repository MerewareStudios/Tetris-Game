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
    [SerializeField] private Button purchaseButton;
    [SerializeField] private RectTransform animationPivot;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;
    [SerializeField] private TextMeshProUGUI detailedInfoText;
    [SerializeField] private Image frameOutline;
    [TextArea] [SerializeField] private string detailedInfo;
    [TextArea] [SerializeField] private string purchaseInfo;
    [System.NonSerialized] private Sequence _colorSequence;

    public PurchaseOption SetPurchase(Const.PurchaseType purchaseType, int price, bool able2Purchase)
    {
        SetButtonVisible(able2Purchase);
        Const.SetPriceStamp(priceText, purchaseButton, purchaseButtonText, able2Purchase, purchaseType, price);
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
    public PurchaseOption SetButtonVisible(bool value)
    {
        purchaseButton.image.enabled = value;
        purchaseButtonText.color = purchaseButtonText.color.SetAlpha(value ? 1.0f : 0.25f);
        return this;
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
