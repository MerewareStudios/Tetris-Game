using System;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using IWI;
using IWI.Tutorial;
using IWI.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PiggyMenu : Menu<PiggyMenu>, IMenu
{
    [Header("End Level Screen")]
    [Header("Bars")]
    [SerializeField] private MarkedProgress _markedProgressPiggy;
    [Header("Currency Displays")]
    [SerializeField] private CurrencyDisplay piggyCurrencyDisplay;
    [Header("Pivots")]
    [SerializeField] private RectTransform _rectTransformPiggyIcon;
    [SerializeField] private RectTransform _coinTarget;
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueText;
    [SerializeField] private Button investButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Transform frame;
    [SerializeField] private Button multiplyButton;
    [Header("Reward")]
    [SerializeField] private RectTransform normalPiggy;
    [SerializeField] private RectTransform rewardedPiggy;
    [SerializeField] private RectTransform rewardedPiggyShakePivot;
    [SerializeField] private Image rewardPiggyGlow;
    [SerializeField] private Material piggyGlowMat;
    [ColorUsage(true, true)] [SerializeField] private Color glowColorStart;
    [ColorUsage(true, true)] [SerializeField] private Color glowColorEnd;
    [Header("Reward")]
    [SerializeField] private Transform clickLocation_Invest;
    [SerializeField] private Transform clickLocation_Continue;
    [SerializeField] private Transform clickLocation_Break;
    [Header("Frame")]
    [SerializeField] private Image ticketImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private Color singleColor;
    [SerializeField] private Color doubleColor;
    [SerializeField] private Transform multProgress;
    public const int PiggyCapIncrease = 10;
    public const int TicketRewardEveryBreak = 8;

    private int _multiplier = 1;
    private Tween _shakeTween;
    
    #region Menu
    public new bool Open(float duration = 0.5f)
    {
        if (base.Open(duration))
        {
            return true;
        }
        Show();
        return false;
    }
    public new bool Close(float duration = 0.25f, float delay = 0.0f)
    {
        if (base.Close())
        {
            return true;
        }
        SwitchToGame();
        return false;
    }
    public new void Show()
    {
        base.Show();
        
        UIManager.MenuMode(true);
        Wallet.ScaleTransactors(1.1f, true);
        _Data = _data;
        
        piggyCurrencyDisplay.Display(_Data.currentMoney, _Data.moneyCapacity);
        _markedProgressPiggy._Progress = _Data.PiggyPercent;

        frame.DOKill();
        frame.localPosition = Vector3.down * 2000.0f;
        frame.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        investButton.gameObject.SetActive(false);
        investButton.targetGraphic.raycastTarget = false;

        if (!_Data.IsFull)
        {
            investButton.transform.DOKill();
            investButton.transform.localScale = Vector3.zero;
            Tween investButtonTween = investButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.4f).SetUpdate(true);
            investButtonTween.OnStart(() =>
            {
                investButton.gameObject.SetActive(true);
            });
            investButtonTween.onComplete = () =>
            {
                investButton.image.raycastTarget = true;
            
                if (ONBOARDING.PIGGY_INVEST.IsNotComplete())
                {
                    Onboarding.ClickOn(clickLocation_Invest.position, Finger.Cam.UI, () =>
                    {
                        investButton.transform.DOKill();
                        investButton.transform.localScale = Vector3.one;
                        investButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    });
                }
            };

        }

        continueButton.gameObject.SetActive(true);
        Transform continueTrans = continueButton.transform;
        continueTrans.DOKill();
        continueTrans.localScale = Vector3.zero;
        if (ONBOARDING.PIGGY_INVEST.IsComplete())
        {
            continueText.text = "LATER";
            continueTrans.localPosition = Vector3.zero;
            continueTrans.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(1.0f).SetUpdate(true);
        }
        
        piggyCurrencyDisplay.gameObject.SetActive(!_Data.IsFull);
        
        _markedProgressPiggy.gameObject.SetActive(!_Data.IsFull);
        rewardedPiggy.gameObject.SetActive(_Data.IsFull);
        if (_Data.IsFull)
        {
            rewardedPiggy.localPosition = Vector3.zero;
            rewardedPiggy.localScale = Vector3.one * 1.5f;
            rewardPiggyGlow.color = glowColorStart;
            piggyGlowMat.SetColor(GameManager.InsideColor, glowColorStart);
        }

        breakButton.gameObject.SetActive(_Data.IsFull);
        
        multiplyButton.gameObject.SetActive(false);
        
        frameImage.DOKill();
        frameImage.color = singleColor;
    }
    #endregion

    public void OnClick_RequestMultiply()
    {
        multiplyButton.targetGraphic.raycastTarget = false;

        if (!Wallet.Consume(Const.Currency.OneAd))
        {
            _shakeTween.Pause();
            AdManager.ShowTicketAd(() =>
            {
                // Wallet.Transaction(Const.Currency.OneAd);
                Mult();
                
            }, false, () =>
            {
                multiplyButton.targetGraphic.raycastTarget = true;

                _shakeTween.Play();
            });
            return;
        }

        Mult();

        void Mult()
        {
            _shakeTween.Pause();
            
            Transform mulTransform = multiplyButton.transform;
            mulTransform.DOKill();
            mulTransform.DOPunchScale(Vector3.one * 0.35f, 0.25f).SetUpdate(true);

            UIManagerExtensions.RequestTicketFromWallet(ticketImage.rectTransform.position, 1, 1,
                (value) =>
                {
                  
                },
                () =>
                {
                    ticketImage.enabled = false;

                    _shakeTween.timeScale = 4.0f;
                    _multiplier = 2;

                    mulTransform.DOKill();
                    mulTransform.DOPunchScale(Vector3.one * 0.25f, 0.25f).SetUpdate(true);

                    frameImage.DOKill();
                    frameImage.DOColor(doubleColor, 0.15f).SetEase(Ease.OutQuad).SetUpdate(true);

                    
                    _shakeTween.Play();
                }, UIEmitter.Cam.UI);
        }
    }

    #region Break
    public void OnClick_BreakPiggyBank()
    {
        _multiplier = 1;
        
        breakButton.targetGraphic.raycastTarget = false;
        breakButton.transform.DOKill();
        breakButton.transform.DOScale(new Vector3(0.0f, 1.0f, 1.0f), 0.25f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            breakButton.gameObject.SetActive(false);
        };


        ticketImage.enabled = true;
        multProgress.localScale = Vector3.one;
        // multiplyText.text = multiplyStr;
        multiplyButton.gameObject.SetActive(true);
        multiplyButton.targetGraphic.raycastTarget = false;
        multiplyButton.transform.localScale = Vector3.zero;
        multiplyButton.transform.DOKill();
        multiplyButton.transform.DOScale(Vector3.one, 0.15f).SetDelay(0.2f).SetEase(Ease.OutBack).SetUpdate(true).onComplete = () =>
        {
            multiplyButton.targetGraphic.raycastTarget = true;
        };
        
        
        rewardPiggyGlow.transform.localScale = Vector3.zero;
        rewardPiggyGlow.transform.DOKill();
        rewardPiggyGlow.transform.DOScale(Vector3.one, 1.5f).SetEase(Ease.OutSine).SetUpdate(true);
        
        
        rewardPiggyGlow.DOColor(glowColorEnd, 1.5f).SetUpdate(true);
        piggyGlowMat.DOColor(glowColorEnd, GameManager.InsideColor, 1.5f).SetUpdate(true);


        const float shakeDur = 2.0f;
        Vector3 vector = Vector3.zero;
        
        
        _shakeTween = DOTween
            .Shake(()=> vector, x=> vector = x, shakeDur, new Vector3(10.0f, 0.0f, 0.0f), 22, 0, false, ShakeRandomnessMode.Harmonic)
            .SetEase(Ease.InSine, 5.0f)
            .SetUpdate(true);

        // float targetScale = 1.0f;
        _shakeTween.onUpdate = () =>
        {
            float elapsed = _shakeTween.ElapsedPercentage();
            multProgress.localScale = new Vector3(1.0f - elapsed, 1.0f, 1.0f);

            float targetScale = 0.75f + elapsed * 0.25f * _multiplier;
            Debug.Log(targetScale + " " + _multiplier);
            rewardedPiggyShakePivot.localScale = Vector3.Lerp(rewardedPiggyShakePivot.localScale, new Vector3(targetScale, targetScale, targetScale), Time.unscaledDeltaTime * 14.0f);
            rewardedPiggyShakePivot.localEulerAngles = new Vector3(0.0f, 0.0f, vector.x *  _shakeTween.ElapsedPercentage());
        };
        
        _shakeTween.onComplete = () =>
        {
            rewardedPiggy.gameObject.SetActive(false);
            _shakeTween?.Kill();

            base.CloseImmediate();

            GiveRewards();
            _Data.breakInstance++;
            _Data.currentMoney.amount = 0;
            _Data.moneyCapacity += PiggyCapIncrease;
        };
        
        if (ONBOARDING.PIGGY_BREAK.IsNotComplete())
        {
            ONBOARDING.PIGGY_BREAK.SetComplete();
            Onboarding.HideFinger();
        }
    }

    public void GiveRewards()
    {
        UIManager.THIS.piggyPS.Play();
        
        int coinReward = (int)(_Data.currentMoney.amount * 0.25f);
        GiveMeta(coinReward, 20, UIManagerExtensions.EmitPiggyRewardCoin);
        int piggyReward = Random.Range(4, 6);
        GiveMeta(piggyReward, 20, UIManagerExtensions.EmitPiggyRewardPiggy);
        int ticketReward = 0;
        
        if (_Data.breakInstance % TicketRewardEveryBreak == 0)
        // if (_Data.breakInstance == 0 || (_Data.breakInstance % 5 == 0 && Helper.IsPossible(0.5f)))
        {
            ticketReward += 1;
        }
        
        GiveMeta(ticketReward, 1, UIManagerExtensions.EmitPiggyRewardTicket);

        SwitchToGame();
    }

    private void SwitchToGame()
    {
        Wallet.ScaleTransactors(1.0f);
        UIManager.MenuMode(false);
        LevelManager.THIS.LoadLevel();
    }

    private void GiveMeta(int amount, int limit, System.Action<Vector3, int, int, System.Action> act)
    {
        amount *= _multiplier;
        if (amount == 0)
        {
            Wallet.TICKET.Scale(1.0f, false);
            return;
        }
        act.Invoke(rewardedPiggy.position, Mathf.Min(amount, limit), amount, null);
    }
    public void OnClick_InvestPiggyBank()
    {
        int amount = Mathf.CeilToInt(Wallet.COIN.Currency.amount * 0.2f);
        amount = Mathf.Clamp(amount, 0, _Data.Remaining);
        if (amount == 0)
        {
            OnClick_ContinuePiggyBank();
            return;
        }
        
        InvestAnimated(new Const.Currency(Wallet.COIN.Currency.type, amount));

        investButton.targetGraphic.raycastTarget = false;
        investButton.transform.DOKill();
        investButton.transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            investButton.gameObject.SetActive(false);
        };
        
        Transform continueTrans = continueButton.transform;
        continueButton.targetGraphic.raycastTarget = false;
        continueTrans.DOKill();
        continueTrans.DOScale(Vector3.zero, 0.45f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            continueButton.gameObject.SetActive(false);
        };
        
        
        if (ONBOARDING.PIGGY_INVEST.IsNotComplete())
        {
            ONBOARDING.PIGGY_INVEST.SetComplete();
            Onboarding.HideFinger();
        }
    }
    public void OnClick_ContinuePiggyBank()
    {
        continueButton.targetGraphic.raycastTarget = false;
        if (ONBOARDING.PIGGY_CONTINUE.IsNotComplete())
        {
            ONBOARDING.PIGGY_CONTINUE.SetComplete();
            // ONBOARDING.LEARNED_ALL_TABS.SetComplete();
            // UIManager.THIS.shop.BarEnabled = true;
            Onboarding.HideFinger();
        }
        this.Close();
    }
    #endregion
    
    [System.NonSerialized] private Data _data;


    #region Fields
    public Data _Data
    {
        set
        {
            _data = value;
            
        }
        get => _data;
    }
   
    #endregion
    #region Animations
    private void PunchPiggyIcon(float amount = 0.25f)
    {
        _rectTransformPiggyIcon.DOKill();
        _rectTransformPiggyIcon.localScale = Vector3.one;
        _rectTransformPiggyIcon.DOPunchScale(Vector3.one * amount, 0.75f, 1).SetUpdate(true);
    }
    #endregion
    #region Invest
    private void AddMoney(int count, float delay = 0.0f)
    {
        _Data.currentMoney.amount += count;
        _Data.currentMoney.amount = Mathf.Clamp(_Data.currentMoney.amount, 0, _Data.moneyCapacity);
        
        _markedProgressPiggy.ProgressAnimated(_data.PiggyPercent, 0.5f, delay, Ease.OutQuad, 
            
            (value) => piggyCurrencyDisplay.Display(_Data.Percent2Money(value), _Data.moneyCapacity), 
            
            () =>
            {
                if (_Data.IsFull)
                {
                    DOVirtual.DelayedCall(0.2f, () =>
                    {
                        breakButton.targetGraphic.raycastTarget = false;
                        breakButton.gameObject.SetActive(true);
                        breakButton.transform.DOKill();
                        breakButton.transform.localScale = Vector3.zero;
                        breakButton.transform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true).onComplete = () =>
                        {
                            if (ONBOARDING.PIGGY_BREAK.IsNotComplete())
                            {
                                Onboarding.ClickOn(clickLocation_Break.position, Finger.Cam.UI, () =>
                                {
                                    breakButton.transform.DOKill();
                                    breakButton.transform.localScale = Vector3.one;
                                    breakButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                                });
                            }
                        };
                        
                        _markedProgressPiggy.gameObject.SetActive(false);
                        piggyCurrencyDisplay.gameObject.SetActive(false);
                        
                        rewardedPiggyShakePivot.localScale = Vector3.one;
                        rewardedPiggyShakePivot.localEulerAngles = Vector3.zero;
                        
                        rewardedPiggy.gameObject.SetActive(true);
                        rewardedPiggy.position = normalPiggy.position;
                        rewardedPiggy.localScale = Vector3.one;
                        rewardedPiggy.DOKill();
                        rewardedPiggy.DOScale(Vector3.one * 1.5f, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true);
                        rewardedPiggy.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutQuad).SetUpdate(true).onComplete =
                            () =>
                            {
                                breakButton.targetGraphic.raycastTarget = true;

                            };
                        
                        rewardPiggyGlow.color = glowColorStart;
                        piggyGlowMat.SetColor(GameManager.InsideColor, glowColorStart);
                    });

                }
                else
                {
                    ShowContinueButton();
                }
            });
    }

    private void ShowContinueButton()
    {
        Transform continueTrans = continueButton.transform;

        continueButton.gameObject.SetActive(true);
        continueText.text = "CONTINUE";
        continueTrans.DOKill();
        continueTrans.position = investButton.transform.position;
        continueTrans.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true).onComplete =
            () =>
            {
                continueButton.targetGraphic.raycastTarget = true;
                            
                            
                if (ONBOARDING.PIGGY_CONTINUE.IsNotComplete())
                {
                    Onboarding.ClickOn(clickLocation_Continue.position, Finger.Cam.UI, () =>
                    {
                        continueTrans.DOKill();
                        continueTrans.localScale = Vector3.one;
                        continueTrans.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    });
                }
                            
            };
    }
    private void InvestAnimated(Const.Currency currency)
    {
        Wallet.Consume(currency);
        UIManagerExtensions.RequestCoinFromWallet(_coinTarget.position, Mathf.Clamp(currency.amount, 1, 15), currency.amount,
        (value) =>
            {
                PunchPiggyIcon(0.2f);
            },
        () =>
            {
                AddMoney(currency.amount, 0.0f);
            });

    }
    #endregion
    
    #region Classes
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public Const.Currency currentMoney;
            [SerializeField] public int moneyCapacity;
            [SerializeField] public int breakInstance;

                
            public Data()
            {
                    
            }
            
            public Data(Data data)
            {
                this.currentMoney = data.currentMoney;
                this.moneyCapacity = data.moneyCapacity;
                this.breakInstance = data.breakInstance;
            }
            
            public float PiggyPercent => currentMoney.amount / (float)moneyCapacity;
            public bool IsFull => currentMoney.amount >= moneyCapacity;
            public Const.Currency Percent2Money(float percent) => new Const.Currency(currentMoney.type, (int)Mathf.Lerp(0.0f, moneyCapacity, percent));
            public int Remaining => moneyCapacity - currentMoney.amount;

            public object Clone()
            {
                return new Data(this);
            }
        } 
    #endregion
}
