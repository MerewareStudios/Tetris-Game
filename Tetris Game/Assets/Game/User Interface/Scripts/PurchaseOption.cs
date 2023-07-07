using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PurchaseOption : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI purchaseButtonText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private TextMeshProUGUI detailedInfoText;
    [TextArea] [SerializeField] private string detailedInfo;

    public PurchaseOption SetPurchase(Const.PurchaseType purchaseType, int price)
    {
        switch (purchaseType)
        {
            case Const.PurchaseType.Coin:
                priceText.text = UIManager.COIN_TEXT + price;
                purchaseButtonText.text = "PURCHASE";
                purchaseButton.image.sprite = Const.THIS.purchaseOptionSprites[0];
                priceText.color = Const.THIS.coinTextColor;
                priceText.fontSharedMaterial = Const.THIS.coinTextMaterial;
                break;
            case Const.PurchaseType.Gem:
                priceText.text = UIManager.GEM_TEXT + price;
                purchaseButtonText.text = "PURCHASE";
                purchaseButton.image.sprite = Const.THIS.purchaseOptionSprites[1];
                priceText.color = Const.THIS.gemTextColor;
                priceText.fontSharedMaterial = Const.THIS.gemTextMaterial;
                break;
            case Const.PurchaseType.Ad:
                priceText.text = UIManager.AD_TEXT;
                purchaseButtonText.text = "FREE";
                purchaseButton.image.sprite = Const.THIS.purchaseOptionSprites[2];
                break;
        }

        return this;
    }
    public PurchaseOption SetDetailedInfo(int gain)
    {
        detailedInfoText.text = string.Format(detailedInfo, gain);
        return this;
    }
}
