using DG.Tweening;
using Internal.Core;
using UnityEngine;

public class OfferScreen : Lazyingleton<OfferScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [System.NonSerialized] public float Timescale = 1.0f;
    [System.NonSerialized] public System.Action OnVisibilityChanged;


    public OfferScreen Open()
    {
        if (canvas.enabled)
        {
            return this;
        }
        
        
        canvas.enabled = true;
        canvasGroup.DOKill();
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
        
        this.gameObject.SetActive(true);
        
        Timescale = 0.0f;
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
        
        
        Timescale = 1.0f;
        OnVisibilityChanged?.Invoke();
        
        return this;
    }

    public void OnClick_Close()
    {
        Close();
    }
    
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }
    
}
