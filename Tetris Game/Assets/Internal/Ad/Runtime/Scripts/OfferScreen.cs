using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class OfferScreen : Lazyingleton<OfferScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [System.NonSerialized] public float TimeScale = 1.0f;
    [System.NonSerialized] public System.Action OnVisibilityChanged;
    [System.NonSerialized] private System.Action _onOfferRejected;
    [System.NonSerialized] private System.Action _onOfferAccepted;


    public OfferScreen Open(System.Action onOfferAccepted = null, System.Action onOfferRejected = null)
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
    
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }

}
