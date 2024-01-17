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
    [SerializeField] private TextMeshProUGUI promoText;
    [SerializeField] private TextMeshProUGUI buttonText;
    [System.NonSerialized] private OfferScreen.OfferType _offerType;

    public void Set(OfferScreen.OfferType offerType)
    {
        this._offerType = offerType;
        
        OfferScreen.MiniData miniData = OfferScreen.THIS.offerData[(int)offerType].miniData;
     
        offerImage.sprite = miniData.icon;
        promoText.text = miniData.promoText;
        buttonText.text = miniData.buttonText;
        
        this.gameObject.SetActive(true);
        thisTransform.DOKill();
        thisTransform.localScale = Vector3.zero;
        thisTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnClick_ShowOffer()
    {
        OfferScreen.THIS.Open(_offerType, OfferScreen.ShowSource.MINI_OFFER);
    }

    public void Halt()
    {
        thisTransform.DOKill();
    }
}
