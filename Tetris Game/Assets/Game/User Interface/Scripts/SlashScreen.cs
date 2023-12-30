using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using Lofelt.NiceVibrations;
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
    [SerializeField] private float closeAppend = 0.75f;
    [SerializeField] private float distance = 3000.0f;
    [SerializeField] private float centerMinHeight = -25.0f;
    [SerializeField] private float centerMaxHeight = 256.0f;
    [SerializeField] private SlashAnimationSettings animationSettingsVictory;
    [SerializeField] private SlashAnimationSettings animationSettingsFail;
    [SerializeField] private SlashAnimationSettings animationSettingsHide;
    [SerializeField] private Color visibleColor;
    [SerializeField] private Color invisibleColor;
    [SerializeField] private GameObject tipParent;
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private GameObject actionButtonParent;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private TextMeshProUGUI failText;
    [System.NonSerialized] private Sequence _sequence;
    [System.NonSerialized] private List<int> _randomTipsIndexes = new();

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
                failText.text = "FAIL";
                slashAnimationSettings = animationSettingsFail;
                break;
            case State.Concede:
                failText.text = "CONCEDE";
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
        
        loadingBar.SetActive(false);
        
        actionButtonParent.gameObject.SetActive(false);
        buttonPanel.gameObject.SetActive(false);

        string tipString = "";
        bool panelVisible = false;

        void ShowReview()
        {
            tipString = Onboarding.THIS.commentTip;
            actionButtonText.text = Onboarding.THIS.reviewText;
    
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                GameManager.THIS.LeaveComment(() =>
                {
                    _sequence.Pause();

                    GameManager.GameTimeScale(0);
                    loadingBar.SetActive(true);
                    actionButtonParent.gameObject.SetActive(false);
                }, (success) =>
                {
                    _sequence.Complete();
            
                    buttonPanel.gameObject.SetActive(false);
                    GameManager.GameTimeScale(1);
                    if (success)
                    {
                        tipText.text = Onboarding.THIS.thanksText;
                        Account.Current.commented = true;

                        UIManagerExtensions.SocialRewardTicket(loadingBar.transform.position, 1, 1, null);
                    }
                });
            });
            panelVisible = true;
        }
        void ShowShare()
        {
            tipString = Onboarding.THIS.shareTip;
            actionButtonText.text = Onboarding.THIS.shareText;
            
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() =>
            {
                GameManager.THIS.ShareTheGame(() =>
                {
                    _sequence.Pause();

                    GameManager.GameTimeScale(0);
                    loadingBar.SetActive(true);
                    actionButtonParent.gameObject.SetActive(false);
                }, (success) =>
                {
                    _sequence.Play();
                
                    buttonPanel.gameObject.SetActive(false);
                    GameManager.GameTimeScale(1);
                    if (success)
                    {
                        tipText.text = Onboarding.THIS.thanksText;
                        
                        UIManagerExtensions.SocialRewardTicket(loadingBar.transform.position, 1, 1, null);
                    }
                });
            });
            panelVisible = true;
        }
        void ShowTip(int? specificIndex = null)
        {
            int index;
            if (specificIndex != null)
            {
                index = specificIndex.Value;
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
                index = _randomTipsIndexes[randomIndex];
                _randomTipsIndexes.RemoveAt(randomIndex);
            }
            tipString = Onboarding.THIS.tips[index];
        }

        if (levelIndex == 1 || (state.Equals(State.Victory) && levelIndex % 6 == 0))
        {
            if (levelIndex == 1)
            {
                ShowTip(14);
            }
            else
            {
                if (Account.Current.commented)
                {
                    ShowShare();
                }
                else
                {
                    if (levelIndex % 12 == 0)
                    {
                        ShowShare();
                    }
                    else
                    {
                        ShowReview();
                    }
                }
            }
        }
        else
        {
            ShowTip();
        }

        float extraAppendWait = 0.0f;
        if (panelVisible)
        {
            buttonPanel.SetActive(true);
            actionButtonParent.gameObject.SetActive(true);
            extraAppendWait += 1.25f;
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
#if CREATIVE
            if (!Const.THIS.creativeSettings.showTip)
            {
                tipParent.SetActive(false);
                return;
            }
#endif
            Audio.Slash_Screen_Open.Play(pitch:0.9f);

            tipParent.SetActive(true);
            tipParent.transform.localScale = Vector3.zero;

            float tipDuration = state.Equals(State.Victory) ? 0.75f : 1.25f;
            tipParent.transform.DOScale(Vector3.one, tipDuration).SetEase(Ease.OutExpo).SetDelay(0.25f);
        });
       
        
        centerImage.color = animationSettings.centerColor;
        topBannerImage.color = animationSettings.bannerGradient.Evaluate(0.0f);

        Tween gradientTop = topBannerImage.DOColor(animationSettings.bannerGradient.Evaluate(1.0f), animationSettings.colorShowDuration).SetEase(Ease.InOutSine);
        
        _sequence?.Kill();
        _sequence = DOTween.Sequence();

        _sequence.SetDelay(delay);
        _sequence.Append(topSlash).Append(expand).Join(gradientTop);
        _sequence.AppendInterval(closeAppend + extraAppendWait);

        _sequence.onComplete += () =>
        {
            Hide(animationSettingsHide, () => 
            {
                HapticManager.Vibrate(HapticPatterns.PresetType.RigidImpact);

                Audio.Slash_Screen_Open.Play(pitch:1.25f);
                UIManagerExtensions.EmitLevelReward(currency, currencyDisplay.iconPivot.position, () =>
                {
                    Close();
                    GameManager.THIS.Deconstruct();
                    UIManager.THIS.AdLayer_OpenPiggyBank();
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
        Fail,
        Concede
    }
}
