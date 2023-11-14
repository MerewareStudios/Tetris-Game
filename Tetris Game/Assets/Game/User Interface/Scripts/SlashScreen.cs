using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SlashScreen : Lazyingleton<SlashScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CurrencyDisplay currencyDisplay;
    [SerializeField] private Image centerImage;
    [SerializeField] private RectTransform topPivot;
    [SerializeField] private Image topBannerImage;
    [SerializeField] private Image backgroudImage;
    [SerializeField] private GameObject victoryImage;
    [SerializeField] private GameObject failImage;
    [SerializeField] private float rewardBackTime = 0.35f;
    [SerializeField] private float distance = 3000.0f;
    [SerializeField] private float centerMinHeight = -25.0f;
    [SerializeField] private float centerMaxHeight = 256.0f;
    [SerializeField] private SlashAnimationSettings animationSettingsVictory;
    [SerializeField] private SlashAnimationSettings animationSettingsFail;
    [SerializeField] private SlashAnimationSettings animationSettingsHide;
    [SerializeField] private Color visibleColor;
    [SerializeField] private Color invisibleColor;
    [SerializeField] private GameObject tipParent;
    [SerializeField] private GameObject actionButtonsPanel;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private TextMeshProUGUI tipText;
    [System.NonSerialized] private Sequence _sequence;
    [System.NonSerialized] private List<int> _randomTipsIndexes = new List<int>();

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
        [SerializeField] public bool tipAvailable;
        [SerializeField] public bool victoryImageActive;
        [SerializeField] public bool failImageActive;
        [SerializeField] public Color centerColor;
    }
    
    public void Show(State state, float delay, Const.Currency currency, int levelIndex)
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
        Show(state, slashAnimationSettings, delay, currency, levelIndex);
    }
    
    private void ResetSelf()
    {
        topPivot.DOKill();
        centerImage.rectTransform.DOKill();
        
        topPivot.anchoredPosition = Vector3.right * -distance;

        topBannerImage.DOKill();
        

        

        centerImage.rectTransform.sizeDelta = new Vector2(distance, centerMinHeight);
    }

    private void Show(State state, SlashAnimationSettings animationSettings, float delay, Const.Currency currency, int levelIndex)
    {
        currencyDisplay.Display(currency);
        
        canvas.enabled = true;
        
        tipParent.SetActive(false);


        float extraHideDelay = 0.0f;
        
        string tipString = "";
        if (state.Equals(State.Victory) && levelIndex % 5 == 1 && !Account.Current.commented)
        {
            tipString = Onboarding.THIS.commentTip;
            actionButtonsPanel.SetActive(true);

            extraHideDelay = 0.0f;

            actionButtonText.text = Onboarding.THIS.reviewText;
            actionButton.onClick.AddListener(GameManager.THIS.LeaveComment);
        }
        else
        {
            if (_randomTipsIndexes.Count == 0)
            {
                for (int i = 0; i < Onboarding.THIS.tips.Length; i++)
                {
                    _randomTipsIndexes.Add(i);
                }
                _randomTipsIndexes.Shuffle();
            }
            int randomIndex = Random.Range(0, _randomTipsIndexes.Count);
            int index = _randomTipsIndexes[randomIndex];
            _randomTipsIndexes.RemoveAt(randomIndex);
            tipString = Onboarding.THIS.tips[index];
            actionButtonsPanel.SetActive(false);
        }
        tipText.text = tipString;

        

        
        backgroudImage.DOKill();
        backgroudImage.color = invisibleColor;
        backgroudImage.DOColor(visibleColor, 0.25f).SetEase(Ease.InOutSine);
        
        
        victoryImage.SetActive(animationSettings.victoryImageActive);
        failImage.SetActive(animationSettings.failImageActive);
        
        Tween topSlash = topPivot.DOAnchorPos(Vector3.zero, animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase, animationSettings.slashShowOvershoot, animationSettings.slashShowPeriod);



        Tween expand = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMaxHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase).SetDelay(animationSettings.expandDelay);
        expand.OnStart(() =>
        {
            tipParent.SetActive(true);
            tipParent.transform.localScale = Vector3.zero;
            tipParent.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(0.2f);
        });
       
        
        centerImage.color = animationSettings.centerColor;
        topBannerImage.color = animationSettings.bannerGradient.Evaluate(0.0f);

        Tween gradientTop = topBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        
        _sequence?.Kill();
        _sequence = DOTween.Sequence();

        _sequence.SetDelay(delay);
        _sequence.Append(topSlash).Append(expand).Join(gradientTop);
        _sequence.AppendInterval(1.25f);

        _sequence.onComplete += () =>
        {
            Hide(animationSettingsHide, () => 
            // DOVirtual.DelayedCall(animationSettingsHide.expandDelay + 0.05f, () =>
            {
                UIManagerExtensions.EmitLevelRewardCoin(currencyDisplay.iconPivot.position, Mathf.Clamp(currency.amount, 1, 15), currency.amount, () =>
                {
                    Close();
                    GameManager.THIS.Deconstruct();
                    UIManager.THIS.AdLayerClick_OpenPiggyBank();
                });
            });
        };
    }
    
    private void Hide(SlashAnimationSettings animationSettings, System.Action emitCoinAction)
    {
        Tween shrink = centerImage.rectTransform.DOSizeDelta(new Vector2(distance, centerMinHeight), animationSettings.expandShowDur).SetEase(animationSettings.expandShowEase);
        Tween topSlash = topPivot.DOAnchorPos(new Vector2(distance, 0.0f), animationSettings.slashShowDur).SetEase(animationSettings.slashShowEase, animationSettings.slashShowOvershoot);
        
        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Join(topSlash).Append(shrink);
        

        DOVirtual.DelayedCall(_sequence.Duration() - rewardBackTime, emitCoinAction.Invoke).SetUpdate(false);
    }

    private void Close()
    {
        canvas.enabled = false;
        this.gameObject.SetActive(false);
    }

    public enum State
    {
        Victory,
        Fail
    }
}
