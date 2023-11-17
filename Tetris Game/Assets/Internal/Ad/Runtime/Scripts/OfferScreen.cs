using System;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferScreen : Lazyingleton<OfferScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private OfferData[] offerData;
    [Header("Visuals")]
    [SerializeField] private OfferPreview[] offerPreviews;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI priceText;
    [System.NonSerialized] public float TimeScale = 1.0f;
    [System.NonSerialized] public System.Action OnVisibilityChanged;
    [System.NonSerialized] private System.Action _onOfferRejected;
    [System.NonSerialized] private System.Action _onOfferAccepted;


    public OfferScreen Open(Type offerType, System.Action onOfferAccepted = null, System.Action onOfferRejected = null)
    {
        if (canvas.enabled)
        {
            return this;
        }

        this._onOfferAccepted = onOfferAccepted;
        this._onOfferRejected = onOfferRejected;
        
        
        canvas.enabled = true;
        canvasGroup.DOKill();
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
        
        this.gameObject.SetActive(true);
        
        TimeScale = 0.0f;
        OnVisibilityChanged?.Invoke();
        
        SetupOffer(offerType);

        return this;
    }

    public OfferScreen Close()
    {
        if (!canvas.enabled)
        {
            return this;
        }
        
        canvasGroup.DOKill();
        canvasGroup.DOFade(0.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            this.gameObject.SetActive(false);
            canvas.enabled = false;
        };        
        
        
        TimeScale = 1.0f;
        OnVisibilityChanged?.Invoke();
        
        return this;
    }

    public void OnClick_Close()
    {
        Close();
        _onOfferRejected?.Invoke();
    }
    
    public void OnClick_Buy()
    {
        
    }

    private void SetupOffer(Type type)
    {
        SetupVisuals(offerData[(int)type]);
    }

    private void SetupVisuals(OfferData data)
    {
        titleText.text = data.title;
        infoText.text = data.detailedInfoStr;
        priceText.text = "2.99$";

        for (int i = 0; i < offerPreviews.Length; i++)
        {
            bool iconEnabled = i < data.previewDatas.Length;
            offerPreviews[i].gameObject.SetActive(iconEnabled);
        
            if (iconEnabled)
            {
                offerPreviews[i].Set(data.previewDatas[i]);
            }
        }
    }
    
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }

    [System.Serializable]
    public enum Type
    {
        RemoveAds,
        CoinPack,
        PiggyCoinPack,
        TicketPack,
        HealthPack,
        BasicChest,
        PrimeChest,
        PrestigeChest,
    }
    
    [System.Serializable]
    public class OfferData
    {
        public delegate string GetPriceFunction(string iapID);
        
        [SerializeField] public OfferScreen.Type offerType;
        [SerializeField] public string iapID;
        [SerializeField] public OfferPreview.PreviewData[] previewDatas;
        [TextArea] [SerializeField] public string title;
        [TextArea] [SerializeField] public string detailedInfoStr;

        public string GetLocalPrice(GetPriceFunction getPriceFunction)
        {
            return getPriceFunction.Invoke(iapID);
        }
    }

    
}
