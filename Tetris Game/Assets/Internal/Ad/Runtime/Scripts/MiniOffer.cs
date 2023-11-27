using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniOffer : MonoBehaviour
{
    [Header("Offer")]
    [SerializeField] private Image offerImage;
    [SerializeField] private TextMeshProUGUI miniPromoText;
    [SerializeField] private TextMeshProUGUI oldPriceText;
    [SerializeField] private TextMeshProUGUI priceText;
    [System.NonSerialized] private OfferScreen.OfferData _offerData;

    public void ShowOffer(OfferScreen.OfferType offerType)
    {
        this._offerData = OfferScreen.THIS.offerData[(int)offerType];
     
        (string oldPrice, string newPrice) = OfferScreen.THIS.GetPriceData(_offerData);
        priceText.text = newPrice;
        oldPriceText.text = oldPrice;
        
        miniPromoText.text = _offerData.miniText;
        offerImage.sprite = _offerData.previewDatas.Last().sprite;
    }

    public void OnClick_ShowOffer()
    {
        OfferScreen.THIS.Open(this._offerData.offerType);
    }
}
