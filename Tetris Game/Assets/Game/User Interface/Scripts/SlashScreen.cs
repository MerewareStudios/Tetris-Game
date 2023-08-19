using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SlashScreen : Singleton<SlashScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CurrencyDisplay currencyDisplay;
    [SerializeField] private Image centerImage;
    [SerializeField] private RectTransform topPivot;
    [SerializeField] private RectTransform bottomPivot;
    [SerializeField] private Image topBannerImage;
    [SerializeField] private Image bottomBannerImage;
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
        [SerializeField] public float slashShowDur;
        [SerializeField] public Ease expandShowEase;
        [SerializeField] public float expandShowDur;
        [SerializeField] public float expandDelay;
        [SerializeField] public float colorShowDuration;
        [SerializeField] public Gradient bannerGradient;
        [SerializeField] public bool bottomPivotActive;
        [SerializeField] public bool victoryImageActive;
        [SerializeField] public bool failImageActive;
    }
    
    public void Show(State state, float delay, Const.Currency currency)
    {
        ResetSelf();
        SlashAnimationSettings slashAnimationSettings = null;
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
        bottomPivot.DOKill();
        centerImage.rectTransform.DOKill();
        
        topPivot.anchoredPosition = Vector3.right * -distance;
        bottomPivot.anchoredPosition = Vector3.right * distance;

        topBannerImage.DOKill();
        bottomBannerImage.DOKill();
        

        topBannerImage.color = animationSettingsVictory.bannerGradient.Evaluate(0.0f);
        bottomBannerImage.color = animationSettingsVictory.bannerGradient.Evaluate(0.0f);

        centerImage.rectTransform.sizeDelta = new Vector2(distance, centerMinHeight);
    }

    private void Show(SlashAnimationSettings animationSettings, float delay, Const.Currency currency)
    {
        currencyDisplay.Display(currency);
        
        canvas.enabled = true;
        
        victoryImage.SetActive(animationSettings.victoryImageActive);
        failImage.SetActive(animationSettings.failImageActive);
        
        bottomPivot.gameObject.SetActive(animationSettings.bottomPivotActive);
        
        Tween topSlash = topPivot.DOAnchorPos(Vector3.zero, animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase);
        Tween bottomSlash = bottomPivot.DOAnchorPos(Vector3.zero, animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase);

       

        Tween expand = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMaxHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase).SetDelay(animationSettings.expandDelay);
        
        expand.OnStart(() =>
        {
            bottomPivot.gameObject.SetActive(true);
        });
        
        Tween gradientTop = topBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        Tween gradientBottom = bottomBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        
        _sequence = DOTween.Sequence();

        _sequence.SetUpdate(true).SetDelay(delay);
        _sequence.Append(topSlash).Join(bottomSlash).Append(expand).Join(gradientTop).Join(gradientBottom);

        _sequence.onComplete += () =>
        {
            Hide(animationSettingsHide, 0.1f);
            UIManagerExtensions.EarnCurrencyScreenStartScale(currency.type, currencyDisplay.iconPivot.position, 1.0f, 1.25f, () =>
            {
                Wallet.Transaction(currency);
                
                PiggyMenu.THIS.Open(0.225f);
            });
        };
    }
    
    private void Hide(SlashAnimationSettings animationSettings, float delay)
    {
        Tween shrink = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMinHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase).SetDelay(animationSettings.expandDelay);
        Tween topSlash = topPivot.DOAnchorPos(new Vector2(distance, 0.0f), animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase);
        Tween bottomSlash = bottomPivot.DOAnchorPos(new Vector2(-distance, 0.0f), animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase);
        
        
        _sequence = DOTween.Sequence();

        _sequence.SetUpdate(true).SetDelay(delay);
        _sequence.Join(topSlash).Join(bottomSlash).Join(shrink);

        _sequence.onComplete += () =>
        {
            canvas.enabled = false;
        };
    }

    public enum State
    {
        Victory,
        Fail
    }
}
