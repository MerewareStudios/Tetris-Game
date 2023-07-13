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
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image centerImage;
    [SerializeField] private RectTransform topPivot;
    [SerializeField] private RectTransform bottomPivot;
    [SerializeField] private Image topBannerImage;
    [SerializeField] private Image bottomBannerImage;
    [SerializeField] private float distance = 3000.0f;
    [SerializeField] private float centerMinHeight = -25.0f;
    [SerializeField] private float centerMaxHeight = 256.0f;
    [FormerlySerializedAs("animationSettings")] [SerializeField] private SlashAnimationSettings animationSettingsVictory;
    [SerializeField] private SlashAnimationSettings animationSettingsFail;
    [System.NonSerialized] private Sequence _sequence;

    [Serializable]
    public struct SlashAnimationSettings
    {
        [SerializeField] public Ease slashShowEase;
        [SerializeField] public float slashShowDur;
        [SerializeField] public Ease expandShowEase;
        [SerializeField] public float expandShowDur;
        [SerializeField] public float expandDelay;
        [SerializeField] public float colorShowDuration;
        [SerializeField] public Gradient bannerGradient;
    }
    
    void Start()
    {
        ResetSelf();
        // Show(animationSettingsVictory);
        Show(animationSettingsFail);
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

    public void Show(SlashAnimationSettings animationSettings)
    {
        Tween topSlash = topPivot.DOAnchorPos(Vector3.zero, animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase);
        Tween bottomSlash = bottomPivot.DOAnchorPos(Vector3.zero, animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase);

        Tween expand = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMaxHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase).SetDelay(animationSettings.expandDelay);
        
        Tween gradientTop = topBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        Tween gradientBottom = bottomBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        
        _sequence = DOTween.Sequence();

        _sequence.SetUpdate(true).SetDelay(1.0f);
        _sequence.Append(topSlash).Join(bottomSlash).Append(expand).Join(gradientTop).Join(gradientBottom);
    }
}
