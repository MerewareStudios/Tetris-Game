using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdBreakScreen : Lazyingleton<AdBreakScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image bannerImage;
    [SerializeField] private Image centerImage;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Button removeAdBreakButton;
    [SerializeField] private Button plusTicketButton;
    [SerializeField] private GameObject adIcon;
    [SerializeField] private GameObject loadingIcon;
    
    [System.NonSerialized] public AdState CurrentAdState = AdState.NONE;
    [System.NonSerialized] private LoadState _currentLoadState;
    
    [System.NonSerialized] private bool _canInteract = false;
    [System.NonSerialized] private bool _byPassing = false;
    [System.NonSerialized] private System.Action _onTimesUp;
    // [System.NonSerialized] private System.Action _onBypassReward;
    [System.NonSerialized] private System.Action _onClick;
    [System.NonSerialized] public static System.Action<bool> onVisibilityChanged;
    
    [System.NonSerialized] private float _duration = 0;
    [System.NonSerialized] private Tween _timerTween;

    public delegate bool ButtonUseCondition();
    private ButtonUseCondition _clickCondition;
    
    [System.NonSerialized] public int TimeScale = 1;
    [System.NonSerialized] public System.Action OnVisibilityChanged;

    
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }

    public bool Visible
    {
        set
        {
            this.gameObject.SetActive(value);
            canvas.enabled = value;
            
            onVisibilityChanged?.Invoke(value);
            
            TimeScale = value ? 0 : 1;
            OnVisibilityChanged?.Invoke();
        }
        get => canvas.enabled;
    }

    public enum AdState
    {
        NONE,
        INTERSTITIAL,
        REWARDED,
    }
    public enum AdInteraction
    {
        SKIP,
        WATCH,
    }
    
    
    public AdBreakScreen SetAdState(AdState adState)
    {
        this.CurrentAdState = adState;
        return this;
    }
    
    public AdBreakScreen SetLoadState(LoadState loadState)
    {
        this._currentLoadState = loadState;

        bool ready = _currentLoadState.Equals(LoadState.Success);
        loadingIcon.SetActive(!ready);
        adIcon.SetActive(ready);
        // warningIcon.SetActive(_currentLoadState.Equals(LoadState.Fail));
        
        if (loadState.Equals(Internal.Core.LoadState.Success) && _canInteract)
        {
            StartTimer();
        }
        
        return this;
    }

    public AdBreakScreen SetVisualData(VisualData visualData)
    {
        fillImage.color = visualData.fillColor;
        centerImage.color = visualData.centerColor;
        bannerImage.color = visualData.bannerColor;
        frameImage.color = visualData.frameColor;
        button.image.sprite = visualData.buttonSprite;
        return this;
    }
    
    public AdBreakScreen SetBackgroundImage(Sprite sprite)
    {
        backgroundImage.sprite = sprite;
        return this;
    }

    public AdBreakScreen SetInfo(string infoStr, string buttonStr)
    {
        // this.topText.text = topStr;
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
    // public AdBreakScreen OnTimesUp(System.Action onTimesUp, System.Action onBypassReward, float duration)
    public AdBreakScreen OnTimesUp(System.Action onTimesUp, float duration)
    {
        this._duration = duration;
        this._onTimesUp = onTimesUp;
        // this._onBypassReward = onBypassReward;
        return this;
    }
    public AdBreakScreen RemoveAdBreakButtonState(bool state)
    {
        this.removeAdBreakButton.gameObject.SetActive(state);
        return this;
    }
    public AdBreakScreen PlusTicketState(bool state)
    {
        this.plusTicketButton.gameObject.SetActive(state);
        return this;
    }
    private void StartTimer()
    {
        if (!_currentLoadState.Equals(LoadState.Success) || !_canInteract)
        {
            return;
        }

        float value = 0.0f;
        _timerTween = DOTween.To(x => value = x, 1.0f, 0.0f, _duration).SetEase(Ease.OutSine, 8.0f).SetUpdate(true);
        _timerTween.onUpdate = () =>
        {
            fillImage.fillAmount = value;
        };
        _timerTween.onComplete = () =>
        {
            _onTimesUp?.Invoke();
            _timerTween = null;
        };


        if (_byPassing)
        {
            Pause();
        }
    }

    public void Open()
    {
        _canInteract = false;

        Visible = true;
        
        fillImage.fillAmount = 1.0f;

        canvasGroup.alpha = 0.0f;
        canvasGroup.DOKill();
        canvasGroup.DOFade(1.0f, 0.25f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            _canInteract = true;
            StartTimer();
        };
    }

    public void Close()
    {
        Stop();

        SetAdState(AdState.NONE);
        _canInteract = false;
        
        canvasGroup.DOKill();
        canvasGroup.DOFade(0.0f, 0.2f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            Visible = false;
        };
    }
    
    public void CloseImmediate()
    {
        Stop();
        SetAdState(AdState.NONE);
        canvasGroup.DOKill();
        _canInteract = false;
        Visible = false;
    }

    private void Stop()
    {
        _timerTween?.Kill();
        _timerTween = null;
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
    }
    
    #region Offer

    private System.Action _onBypass;
    
    public AdBreakScreen OnByPass(System.Action onBypass)
    {
        this._onBypass = onBypass;
        return this;
    }
    
    public void ByPassInProgress()
    {
        _byPassing = true;
        Pause();
    }

    public void RevokeByPass()
    {
        _byPassing = false;
        Restart();
    }

    public void InvokeByPass()
    {
        _byPassing = false;
        Stop();
        Close();
        _onBypass?.Invoke();
    }
    
    #endregion
    
    [System.Serializable]
    public struct VisualData
    {
        [SerializeField] public Color frameColor;
        [SerializeField] public Color bannerColor;
        [SerializeField] public Color centerColor;
        [SerializeField] public Color fillColor;
        [SerializeField] public Sprite buttonSprite;
    }
}
