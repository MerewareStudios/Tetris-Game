using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MiniOffer : MonoBehaviour
{
    [Header("Offer")]
    [SerializeField] private Transform thisTransform;
    [SerializeField] private Image offerImage;
    [SerializeField] private TextMeshProUGUI miniPromoText;
    [SerializeField] private TextMeshProUGUI oldPriceText;
    [SerializeField] private TextMeshProUGUI priceText;
    [System.NonSerialized] private OfferScreen.OfferData _offerData;
    [System.NonSerialized] private OfferScreen.AdPlacement _adPlacement;

    public void ShowOffer(OfferScreen.OfferType offerType, OfferScreen.AdPlacement adPlacement)
    {
        this._offerData = OfferScreen.THIS.offerData[(int)offerType];
        this._adPlacement = adPlacement;
     
        (string oldPrice, string newPrice) = OfferScreen.THIS.GetPriceData(_offerData);
        priceText.text = newPrice;
        oldPriceText.text = oldPrice;
        
        miniPromoText.text = _offerData.miniText;
        offerImage.sprite = _offerData.previewDatas.Last().sprite;
        
        this.gameObject.SetActive(true);
        thisTransform.DOKill();
        thisTransform.localScale = Vector3.zero;
        thisTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnClick_ShowOffer()
    {
        OfferScreen.THIS.Open(this._offerData.offerType, _adPlacement);
    }

    public void Halt()
    {
        thisTransform.DOKill();
    }
}
