using System;
using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class OfferScreen : Lazyingleton<OfferScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private OfferData[] offerData;
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

    public void SetupOffer(Type type)
    {
        
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
        [SerializeField] public Sprite[] offerIcons;
        [TextArea] [SerializeField] public string title;
        [TextArea] [SerializeField] public string detailedInfoStr;

        public string GetLocalPrice(GetPriceFunction getPriceFunction)
        {
            return getPriceFunction.Invoke(iapID);
        }
    }

    
}
