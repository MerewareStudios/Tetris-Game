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

    public void SetPurchase(Const.PurchaseType purchaseType, int priceAmount)
    {
        switch (purchaseType)
        {
            case Const.PurchaseType.Coin:
                priceText.text = UIManager.COIN_TEXT + priceAmount;
                purchaseButtonText.text = "PURCHASE";
                purchaseButton.image.sprite = Const.THIS.purchaseOptionSprites[0];
                break;
            case Const.PurchaseType.Gem:
                priceText.text = UIManager.GEM_TEXT + priceAmount;
                purchaseButtonText.text = "PURCHASE";
                purchaseButton.image.sprite = Const.THIS.purchaseOptionSprites[1];
                break;
            case Const.PurchaseType.Ad:
                priceText.text = UIManager.AD_TEXT;
                purchaseButtonText.text = "FREE";
                purchaseButton.image.sprite = Const.THIS.purchaseOptionSprites[2];
                break;
        }
    }

}
