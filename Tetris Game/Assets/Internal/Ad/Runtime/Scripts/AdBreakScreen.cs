using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdBreakScreen : Lazyingleton<AdBreakScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI topText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private GameObject adPurchaseWindow;
    [SerializeField] private GameObject ticketPurchaseWindow;
    [System.NonSerialized] private bool _canInteract = false;
    [System.NonSerialized] private System.Action _onTimesUp;
    [System.NonSerialized] private System.Action _onClick;
    
    [System.NonSerialized] private int _duration = 0;
    [System.NonSerialized] private Tween _timerTween;

    public delegate bool ButtonUseCondition();
    private ButtonUseCondition _clickCondition;


    public AdBreakScreen SetInfo(string topStr, string infoStr, string buttonStr)
    {
        this.topText.text = topStr;
        this.infoText.text = infoStr;
        this.buttonText.text = buttonStr;
        return this;
    }
    public AdBreakScreen OnClick(System.Action onButtonClicked, ButtonUseCondition clickCondition)
    {
        this._onClick = onButtonClicked;
        this._clickCondition = clickCondition;
        return this;
    }
    public AdBreakScreen OnTimesUp(System.Action onTimesUp, int duration)
    {
        this._duration = duration;
        this._onTimesUp = onTimesUp;
        return this;
    }
    public AdBreakScreen SetPurchaseWindows(bool adPurchaseEnable, bool ticketPurchaseEnable)
    {
        this.adPurchaseWindow.SetActive(adPurchaseEnable);
        this.ticketPurchaseWindow.SetActive(ticketPurchaseEnable);
        return this;
    }
    private void StartTimer()
    {
        float value = 0.0f;
        _timerTween = DOTween.To(x => value = x, 1.0f, 0.0f, _duration).SetEase(Ease.OutSine, 8.0f).SetUpdate(true);
        _timerTween.onUpdate = () =>
        {
            fillImage.fillAmount = value;
        };
        _timerTween.onComplete = _onTimesUp.Invoke;
    }

    public void Open()
    {
        _canInteract = false;

        this.gameObject.SetActive(true);
        canvas.enabled = true;

        canvasGroup.alpha = 0.0f;
        canvasGroup.DOKill();
        canvasGroup.DOFade(1.0f, 0.25f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            _canInteract = true;
        };
        
        StartTimer();
    }

    public void Close()
    {
        _canInteract = false;
        
        canvasGroup.DOKill();
        canvasGroup.DOFade(0.0f, 0.25f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            this.gameObject.SetActive(false);
            canvas.enabled = false;
        };
    }
    
    public void CloseImmediate()
    {
        Stop();
        this.gameObject.SetActive(false);
        canvas.enabled = false;
    }

    private void Stop()
    {
        _timerTween?.Kill();
        canvas.DOKill();
    }
    private void Pause()
    {
        _timerTween?.Pause();
    }
    private void Restart()
    {
        _timerTween?.Restart();
    }
    public void OnClick()
    {
        if (!_canInteract)
        {
            return;
        }
        if (!_clickCondition.Invoke())
        {
            return;
        }

        _canInteract = false;
        _onClick?.Invoke();
        Stop();
    }
    public void OnClick_PurchaseAd()
    {
        if (!_canInteract)
        {
            return;
        }
        Pause();
    }
    public void OnClick_PurchaseTicket()
    {
        if (!_canInteract)
        {
            return;
        }
        Pause();
    }

    public void OnPurchaseFailed()
    {
        Restart();
    }
    public void OnPurchaseSuccessful()
    {
        _onClick?.Invoke();
        Stop();
    }
}
