using System;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
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
    [Header("Reward")]
    [SerializeField] private Canvas piggySortedCanvas;
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
        UIManager.MenuMode(false);
        Wallet.ScaleTransactors(1.0f);
        LevelManager.THIS.LoadLevel();
        return false;
    }
    public new void Show()
    {
        base.Show();
        
        UIManager.MenuMode(true);
        Wallet.ScaleTransactors(1.5f, true);
        _Data = _data;
        
        piggyCurrencyDisplay.Display(_Data.currentMoney);
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
            
                if (ONBOARDING.LEARN_TO_INVEST.IsNotComplete())
                {
                    Onboarding.ClickOn(clickLocation_Invest.position, false, () =>
                    {
                        investButton.transform.DOKill();
                        investButton.transform.localScale = Vector3.one;
                        investButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    });
                }
            };

        }
        
        continueButton.transform.DOKill();
        continueButton.transform.localScale = Vector3.zero;
        if (ONBOARDING.LEARN_TO_INVEST.IsComplete())
        {
            continueText.text = "LATER";
            continueButton.transform.localPosition = Vector3.zero;
            continueButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(1.0f).SetUpdate(true);
        }
        
        
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
    }
    #endregion

    #region Break
    public void OnClick_BreakPiggyBank()
    {
        breakButton.targetGraphic.raycastTarget = false;
        breakButton.transform.DOKill();
        breakButton.transform.DOScale(new Vector3(0.0f, 1.0f, 1.0f), 0.25f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            breakButton.gameObject.SetActive(false);
        };
        
        
        rewardPiggyGlow.transform.localScale = Vector3.zero;
        rewardPiggyGlow.transform.DOKill();
        rewardPiggyGlow.transform.DOScale(Vector3.one, 1.5f).SetEase(Ease.OutSine).SetUpdate(true);
        
        
        rewardPiggyGlow.DOColor(glowColorEnd, 1.5f).SetUpdate(true);
        piggyGlowMat.DOColor(glowColorEnd, GameManager.InsideColor, 1.5f).SetUpdate(true);


        rewardedPiggyShakePivot.DOKill();
        rewardedPiggyShakePivot.localEulerAngles = Vector3.zero;
        rewardedPiggyShakePivot.DOScale(Vector3.one * 1.2f, 1.5f).SetEase(Ease.InSine).SetUpdate(true);
        Tween shakeTween = rewardedPiggyShakePivot.DOPunchRotation(new Vector3(0.0f, 0.0f, 10.0f), 1.5f, 25).SetEase(Ease.InOutBounce).SetUpdate(true);
        shakeTween.SetAutoKill(false);
        shakeTween.Complete();
        shakeTween.PlayBackwards();

        DOVirtual.DelayedCall(1.5f, () =>
        {
            rewardedPiggy.gameObject.SetActive(false);
            shakeTween?.Kill();
            
            Particle.Piggy_Break_Ps.Emit(100, rewardedPiggyShakePivot.position);

            base.CloseImmediate();

            GiveRewards();
            _Data.breakInstance++;
            _Data.currentMoney.amount = 0;
            _Data.moneyCapacity += 25;
        });
        
        if (ONBOARDING.LEARN_TO_BREAK.IsNotComplete())
        {
            ONBOARDING.LEARN_TO_BREAK.SetComplete();
            Onboarding.HideFinger();
        }
    }

    public void GiveRewards()
    {

        List<PiggyMenu.PiggyReward> rewardDatas = new List<PiggyMenu.PiggyReward>();
        
        
        int capIndex = _Data.moneyCapacity / 25;
        
        // case PiggyReward.Type.Coin:
        int coinPercent = (int)(50 * 0.2f);
        int coinCount = Random.Range(coinPercent, coinPercent + 10);
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Coin, coinCount));
        Wallet.COIN.Transaction(coinCount);
        // case PiggyReward.Type.PiggyCoin:
        int piggyCoinCount = Random.Range(capIndex * 2, capIndex * 3);
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.PiggyCoin, piggyCoinCount));
        Wallet.PIGGY.Transaction(piggyCoinCount);
        // case PiggyReward.Type.Ad:
        Debug.Log(_Data.breakInstance + " " + (_Data.breakInstance % 3));
        if (_Data.breakInstance % 3 == 0)
        {
            Helper.IsPossible(0.5f, () =>
            {
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Ad, 1));
                Wallet.AD.Transaction(1);
            });
        }
        // case PiggyReward.Type.Shield:
        if (capIndex >= 3)
        {
            Helper.IsPossible(0.75f, () =>
            {
                int shieldAmount = capIndex * 5;
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Shield, shieldAmount));
                Warzone.THIS.Player.shield.AddOnly(shieldAmount);
            });
        }
        // case PiggyReward.Type.Heart:
        if (capIndex >= 3)
        {
            Helper.IsPossible(0.85f, () =>
            {
                int heartAmount = capIndex;
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Heart, heartAmount));
                Warzone.THIS.Player._CurrentHealth += heartAmount;
            });
        }
        // case PiggyReward.Type.Medkit:
        if (capIndex >= 4)
        {
            Helper.IsPossible(0.5f, () =>
            {
                int medkitAmount = capIndex;
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Medkit, medkitAmount));
                Warzone.THIS.Player._CurrentHealth += medkitAmount * 10;
            });
        }
        // case PiggyReward.Type.Protection:
        if (capIndex >= 5)
        {
            Helper.IsPossible(0.5f, () =>
            {
                int protectionAmount = capIndex;
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, protectionAmount));
                Warzone.THIS.Player.shield.AddOnly(protectionAmount * 10);
            });
        }
        // case PiggyReward.Type.MaxStack:
        if (capIndex >= 3)
        {
            Helper.IsPossible(0.2f, () =>
            {
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.MaxStack, 1));
                Board.THIS._Data.maxStack++;
            });
        }     
        // case PiggyReward.Type.PiggyCapacity:
        if (capIndex >= 6)
        {
            Helper.IsPossible(0.25f, () =>
            {
                rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.PiggyCapacity, 25));
                _Data.moneyCapacity += 25;
            });
        }     
        RewardScreen.THIS.Show(rewardDatas);
    }
    public void OnClick_InvestPiggyBank()
    {
        int amount = Mathf.CeilToInt(Wallet.COIN.Currency.amount * 0.2f);
        amount = Mathf.Clamp(amount, 0, _Data.Remaining);
        if (amount == 0)
        {
            return;
        }
        
        InvestAnimated(new Const.Currency(Wallet.COIN.Currency.type, amount));

        investButton.targetGraphic.raycastTarget = false;
        investButton.transform.DOKill();
        investButton.transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            investButton.gameObject.SetActive(false);
        };
        
        continueButton.targetGraphic.raycastTarget = false;
        continueButton.transform.DOKill();
        continueButton.transform.DOScale(Vector3.zero, 0.45f).SetEase(Ease.InBack).SetUpdate(true).onComplete = () =>
        {
            continueButton.gameObject.SetActive(false);
        };
        
        
        if (ONBOARDING.LEARN_TO_INVEST.IsNotComplete())
        {
            ONBOARDING.LEARN_TO_INVEST.SetComplete();
            Onboarding.HideFinger();
        }
    }
    public void OnClick_ContinuePiggyBank()
    {
        continueButton.targetGraphic.raycastTarget = false;
        if (ONBOARDING.LEARN_TO_CONTINUE.IsNotComplete())
        {
            ONBOARDING.LEARN_TO_CONTINUE.SetComplete();
            ONBOARDING.EARN_SHOP_POINT.SetComplete();
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
   
    public int MaxPiggyLevel
    {
        set
        {
            int newValue = Mathf.Clamp(value, 0, 9);
            _Data.maxPiggyLevel = newValue;
        }
        get => _Data.maxPiggyLevel;
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
        
        _markedProgressPiggy.ProgressAnimated(_data.PiggyPercent, 0.35f, delay, Ease.OutQuad, 
            
            (value) => piggyCurrencyDisplay.Display(_Data.Percent2Money(value)), 
            
            () =>
            {
                if (_Data.IsFull)
                {
                    DOVirtual.DelayedCall(0.15f, () =>
                    {
                        breakButton.targetGraphic.raycastTarget = false;
                        breakButton.gameObject.SetActive(true);
                        breakButton.transform.DOKill();
                        breakButton.transform.localScale = Vector3.zero;
                        breakButton.transform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true).onComplete = () =>
                        {
                            if (ONBOARDING.LEARN_TO_BREAK.IsNotComplete())
                            {
                                Onboarding.ClickOn(clickLocation_Break.position, false, () =>
                                {
                                    breakButton.transform.DOKill();
                                    breakButton.transform.localScale = Vector3.one;
                                    breakButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                                });
                            }
                        };
                        
                        _markedProgressPiggy.gameObject.SetActive(false);
                        
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
        continueButton.gameObject.SetActive(true);
        continueText.text = "CONTINUE";
        continueButton.transform.DOKill();
        continueButton.transform.position = investButton.transform.position;
        continueButton.transform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true).onComplete =
            () =>
            {
                continueButton.targetGraphic.raycastTarget = true;
                            
                            
                if (ONBOARDING.LEARN_TO_CONTINUE.IsNotComplete())
                {
                    Onboarding.ClickOn(clickLocation_Continue.position, false, () =>
                    {
                        continueButton.transform.DOKill();
                        continueButton.transform.localScale = Vector3.one;
                        continueButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
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
            [SerializeField] public int maxPiggyLevel = 9;
            [SerializeField] public Const.Currency currentMoney;
            [SerializeField] public int moneyCapacity;
            [SerializeField] public int breakInstance;

            public Data()
            {
                    
            }
            
            public Data(Data data)
            {
                this.maxPiggyLevel = data.maxPiggyLevel;
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
    [System.Serializable]
    public class PiggyReward : ICloneable
    {
        [SerializeField] public Type type;
        [SerializeField] public int amount;
        
        
        public PiggyReward()
        {
                
        }
        public PiggyReward(PiggyReward piggyReward)
        {
            this.type = piggyReward.type;
            this.amount = piggyReward.amount;
        }
        public PiggyReward(PiggyReward.Type type, int amount)
        {
            this.type = type;
            this.amount = amount;
        }
        
        public object Clone()
        {
            return new PiggyReward(this);
        }

        public enum Type
        {
            Coin,
            PiggyCoin,
            Ad,
            Shield,
            Heart,
            Medkit,
            Protection,
            MaxStack,
            PiggyCapacity,
            // Damage,
            // Firerate,
            // Splitshot,
            // Weapon
        }
    } 
    #endregion
}
