using System;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class SlashScreen : Lazyingleton<SlashScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CurrencyDisplay currencyDisplay;
    [SerializeField] private Image centerImage;
    [SerializeField] private RectTransform topPivot;
    [SerializeField] private Image topBannerImage;
    [SerializeField] private GameObject victoryImage;
    [SerializeField] private GameObject failImage;
    [SerializeField] private float distance = 3000.0f;
    [SerializeField] private float centerMinHeight = -25.0f;
    [SerializeField] private float centerMaxHeight = 256.0f;
    [SerializeField] private SlashAnimationSettings animationSettingsVictory;
    [SerializeField] private SlashAnimationSettings animationSettingsFail;
    [SerializeField] private SlashAnimationSettings animationSettingsHide;
    [System.NonSerialized] private Sequence _sequence;

    [Serializable]
    public class SlashAnimationSettings
    {
        [SerializeField] public Ease slashShowEase;
        [SerializeField] public float slashShowOvershoot;
        [SerializeField] public float slashShowPeriod;
        [SerializeField] public float slashShowDur;
        [SerializeField] public Ease expandShowEase;
        [SerializeField] public float expandShowDur;
        [SerializeField] public float expandDelay;
        [SerializeField] public float colorShowDuration;
        [SerializeField] public Gradient bannerGradient;
        [SerializeField] public bool bottomPivotActive;
        [SerializeField] public bool victoryImageActive;
        [SerializeField] public bool failImageActive;
        [SerializeField] public Color centerColor;
    }
    
    public void Show(State state, float delay, Const.Currency currency)
    {
        this.gameObject.SetActive(true);
        SlashAnimationSettings slashAnimationSettings = null;
        ResetSelf();
        switch (state)
        {
            case State.Victory:
                slashAnimationSettings = animationSettingsVictory;
                break;
            case State.Fail:
                slashAnimationSettings = animationSettingsFail;
                break;
        }
        Show(slashAnimationSettings, delay, currency);
    }
    
    private void ResetSelf()
    {
        topPivot.DOKill();
        centerImage.rectTransform.DOKill();
        
        topPivot.anchoredPosition = Vector3.right * -distance;

        topBannerImage.DOKill();
        

        

        centerImage.rectTransform.sizeDelta = new Vector2(distance, centerMinHeight);
    }

    private void Show(SlashAnimationSettings animationSettings, float delay, Const.Currency currency)
    {
        currencyDisplay.Display(currency);
        
        canvas.enabled = true;
        
        victoryImage.SetActive(animationSettings.victoryImageActive);
        failImage.SetActive(animationSettings.failImageActive);
        
        Tween topSlash = topPivot.DOAnchorPos(Vector3.zero, animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase, animationSettings.slashShowOvershoot, animationSettings.slashShowPeriod);

       

        Tween expand = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMaxHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase).SetDelay(animationSettings.expandDelay);
        
        centerImage.color = animationSettings.centerColor;
        topBannerImage.color = animationSettings.bannerGradient.Evaluate(0.0f);

        Tween gradientTop = topBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        
        _sequence = DOTween.Sequence();

        _sequence.SetUpdate(true).SetDelay(delay);
        _sequence.Append(topSlash).Append(expand).Join(gradientTop);

        _sequence.onComplete += () =>
        {
            Hide(animationSettingsHide, 0.2f);
            DOVirtual.DelayedCall(0.45f, () =>
            {
                UIManagerExtensions.EmitLevelRewardCoin(currencyDisplay.iconPivot.position, Mathf.Clamp(currency.amount, 1, 15), currency.amount, () =>
                {
                    SaveManager.THIS.Save();
                    GameManager.THIS.Deconstruct();
                    PiggyMenu.THIS.Open(0.225f);
                });
            });
        };
    }
    
    private void Hide(SlashAnimationSettings animationSettings, float delay)
    {
        Tween shrink = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMinHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase).SetDelay(animationSettings.expandDelay);
        Tween topSlash = topPivot.DOAnchorPos(new Vector2(distance, 0.0f), animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase, animationSettings.slashShowOvershoot);
        
        
        _sequence = DOTween.Sequence();

        _sequence.Join(topSlash).Join(shrink);
        _sequence.SetUpdate(true).SetDelay(delay);

        _sequence.onComplete += () =>
        {
            canvas.enabled = false;
            this.gameObject.SetActive(false);
        };
    }

    public enum State
    {
        Victory,
        Fail
    }
}
