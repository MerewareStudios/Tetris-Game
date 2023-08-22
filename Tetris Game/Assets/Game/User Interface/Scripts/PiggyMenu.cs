using System;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PiggyMenu : Menu<PiggyMenu>, IMenu
{
    [Header("End Level Screen")]
    [Header("Bars")]
    [SerializeField] private MarkedProgress _markedProgressPiggy;
    // [Header("Buttons")]
    // [SerializeField] private RewardButton[] piggyRewardButtons;
    [Header("Currency Displays")]
    [SerializeField] private CurrencyDisplay piggyCurrencyDisplay;
    [Header("Pivots")]
    [SerializeField] private RectTransform _rectTransformPiggyIcon;
    [SerializeField] private RectTransform _coinTarget;
    // [SerializeField] private RectTransform _rewardsCenter;
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button investButton;
    [SerializeField] private Button breakButton;
    [SerializeField] private Transform frame;
    [Header("Reward")]
    // [SerializeField] private Animator piggyAnimator;
    [SerializeField] private RectTransform normalPiggy;
    [SerializeField] private RectTransform rewardedPiggy;
    [SerializeField] private RectTransform rewardedPiggyShakePivot;
    [SerializeField] private Image rewardPiggyGlow;
    [SerializeField] private Material piggyGlowMat;
    [ColorUsage(true, true)] [SerializeField] private Color glowColorStart;
    [ColorUsage(true, true)] [SerializeField] private Color glowColorEnd;

    
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
        return false;
    }
    private void Show()
    {
        UIManager.MenuMode(true);
        Wallet.ScaleTransactors(1.5f, true);
        _Data = _data;
        
        piggyCurrencyDisplay.Display(_Data.currentMoney);
        _markedProgressPiggy._Progress = _Data.PiggyPercent;
        CanBreak = CanBreak;

        frame.DOKill();
        frame.localPosition = Vector3.down * 2000.0f;
        frame.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

        investButton.gameObject.SetActive(false);
        investButton.targetGraphic.raycastTarget = false;

        if (!CanBreak)
        {
            investButton.transform.DOKill();
            investButton.transform.localScale = Vector3.zero;
            investButton.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack).SetDelay(0.4f).SetUpdate(true).OnStart(() =>
            {
                investButton.gameObject.SetActive(true);
                investButton.image.raycastTarget = true;
            });
            
        }
        
        continueButton.transform.DOKill();
        continueButton.transform.localScale = Vector3.zero;
        continueButton.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetDelay(1.0f).SetUpdate(true);
        
        
        _markedProgressPiggy.gameObject.SetActive(true);
        rewardedPiggy.gameObject.SetActive(false);
    }
    #endregion

    #region Break
    public bool CanBreak
    {
        set
        {
            breakButton.gameObject.SetActive(value);
        }
        get => _Data.IsFull;
    }

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
            Particle.Piggy_Break_Ps.Emit(100, rewardedPiggyShakePivot.position);

            base.CloseImmediate();
            RewardScreen.THIS.ShowFakeCards();
        });
    }
    public void OnClick_InvestPiggyBank()
    {
        int amount = Mathf.CeilToInt(Wallet.COIN.Currency.amount * 0.2f);
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
    }
    public void OnClick_ContinuePiggyBank()
    {
        this.Close();
        LevelManager.THIS.LoadLevel();
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
    // #region Rewards
    // private void DisplayRewards()
    // {
    //     foreach (var button in piggyRewardButtons)
    //     {
    //         button.gameObject.SetActive(false);
    //     }
    //     
    //     _Data.rewards.Shuffle();
    //
    //     List<Vector3> positions = CircleLayoutGroup.GetPoints(_rewardsCenter.position, _Data.RewardCount,  _Data.RewardCount.Direction(), _Data.RewardCount.Radius());
    //
    //     for (int i = 0; i < _Data.RewardCount; i++)
    //     {
    //         RewardButton rewardButton = piggyRewardButtons[i];
    //         PiggyReward piggyReward = _Data.rewards[i];
    //         rewardButton.OnClick(() =>
    //         {
    //             _Data.rewards.Remove(piggyReward);
    //             rewardButton.ShowReward(piggyReward);
    //
    //             if (_Data.rewards.Count <= 0)
    //             {
    //                 // DOVirtual.DelayedCall(1.25f, CloseAction).SetUpdate(true);
    //             }
    //
    //         }).Show(_rewardsCenter.position, positions[i], i * 0.1f);
    //     }
    // }

    
    // #endregion
    #region Invest
    private void AddMoney(int count, float delay = 0.0f)
    {
        _Data.currentMoney.amount += count;
        _Data.currentMoney.amount = Mathf.Clamp(_Data.currentMoney.amount, 0, _Data.moneyCapacity);
        
        _markedProgressPiggy.ProgressAnimated(_data.PiggyPercent, Mathf.Clamp(count / 100.0f, 0.2f, 0.8f), delay, Ease.OutQuad, 
            
            (value) => piggyCurrencyDisplay.Display(_Data.Percent2Money(value)), 
            
            () =>
            {
                if (_Data.IsFull)
                {
                    breakButton.targetGraphic.raycastTarget = false;
                    breakButton.gameObject.SetActive(true);
                    breakButton.transform.DOKill();
                    breakButton.transform.localScale = Vector3.one;
                    breakButton.transform.DOPunchScale(Vector3.one * 0.25f, 0.45f, 1).SetUpdate(true);
                    
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
                            // rewardPiggyGlow.color = glowColorStart;
                            // rewardPiggyGlow.DOColor(glowColorEnd, 0.4f).SetUpdate(true)
                    // Money = 0;

                    // _markedProgressPiggy.ProgressAnimated(_data.PiggyPercent, 0.2f, 0.25f, Ease.Linear,null, () =>
                    // {
                    // Money = 0;
                    // if (excess > 0)
                    // {
                    //     AddMoney(excess, 0.15f);
                    // }
                    // else
                    // {
                    // ShowPiggyScreenButtons(_Data.piggyLevel > 0, true);
                    // }

                    // });
                }
                else
                {
                    continueButton.gameObject.SetActive(true);
                    continueButton.transform.DOKill();
                    continueButton.transform.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack).SetUpdate(true).onComplete =
                        () =>
                        {
                            continueButton.targetGraphic.raycastTarget = true;
                        };
                    

                    // ShowPiggyScreenButtons(_Data.piggyLevel > 0, true);
                }
            });
    }
    // private void InvestCoins(Const.Currency currency)
    // {
    //     ShowScreen(false, true, false);
    //     ShowPiggyScreenButtons(false, false);
    //     
    //     DOVirtual.DelayedCall(0.4f, () =>
    //     {
    //         InvestAnimation(currency);
    //     });
    // }
    private void InvestAnimated(Const.Currency currency)
    {
        Wallet.Consume(currency);
        const int maxCoin = 6;
        
        int count = Mathf.Min(currency.amount, maxCoin);
        float posDif = 0.3f;
            
        Vector3 screenStart = Wallet.IconPosition(currency.type);
        Vector3 screenEnd = _coinTarget.position;
        Vector3 screenDrag = screenStart;
        screenDrag.x = screenEnd.x;
        screenDrag.y = screenEnd.y + 0.2f * (maxCoin - count + 1);
            
        for (int i = 0; i < count; i++)
        {
            screenDrag.y += posDif;
    
            int index = i;
            UIManagerExtensions.DragCoin(screenStart, screenDrag, screenEnd, 0.7f + i * -0.025f, (() =>
            {
                PunchPiggyIcon(0.2f);
                    
                if (index == count - 1)
                {
                    AddMoney(currency.amount, 0.0f);
                }
    
            })).SetDelay(i * 0.125f);
        }
    }
    #endregion
    
    #region Option Buttons

    // private Const.Currency InvestWithStrategy(Const.Currency currency)
    // {
    //     if (currency.amount <= 3)
    //     {
    //         return currency;
    //     }
    //     currency.amount = Mathf.FloorToInt(currency.amount * 0.5f);
    //     if (currency.amount % 2 == 1)
    //     {
    //         currency.amount++;
    //     }
    //
    //     currency.amount = Mathf.Clamp(currency.amount, 1, 50);
    //     return currency;
    // }
    // public void Option_Keep()
    // {
    //     CloseAction();
    // }
    // public void Option_Invest()
    // {
    //     InvestCoins(InvestWithStrategy(Wallet.COIN.Currency));
    // }
    //
    // public void Option_InvestFree()
    // {
    //     Debug.LogWarning("Watch Ad - Not Implemented, invest");
    //     InvestCoins(_Data.freeInvestment);
    // }
    // public void Option_JustOpen()
    // {
    //     OnClick_Break();
    // }
    // public void Option_OpenRewards()
    // {
    //     DisplayRewards();
    // }
    
    #endregion
    #region Piggy Screen Buttons
   
    // public void OnClick_Continue()
    // {
    //     CloseAction();
    // }
    // public void OnClick_Break()
    // {
    //     _Data.GenerateRewards();
    //     DisplayRewards();
    // }
    #endregion
    #region Classes
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public int maxPiggyLevel = 9;
            [SerializeField] public Const.Currency currentMoney;
            [SerializeField] public int moneyCapacity;
            [SerializeField] public Const.Currency freeInvestment;
            [SerializeField] public List<PiggyReward> rewards = new();

            public Data()
            {
                    
            }
            
            public Data(Data data)
            {
                this.maxPiggyLevel = data.maxPiggyLevel;
                this.currentMoney = data.currentMoney;
                this.moneyCapacity = data.moneyCapacity;
                this.freeInvestment = data.freeInvestment;
                this.rewards.CopyFrom(data.rewards);
            }

            
            // public void GenerateRewards()
            // {
                // if (piggyLevel < 1) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.Gem, UnityEngine.Random.Range(5, 26)));
                // if (piggyLevel < 2) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.Coin, UnityEngine.Random.Range(5, 26)));
                // if (piggyLevel < 3) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.Shield, UnityEngine.Random.Range(5, 16)));
                // if (piggyLevel < 4) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.Heart, UnityEngine.Random.Range(5, 11)));
                // if (piggyLevel < 5) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.Ad, UnityEngine.Random.Range(1, 3)));
                // if (piggyLevel < 6) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.MaxStack, 1));
                // // if (piggyLevel < 7) return;
                // // rewards.Add(new PiggyReward(PiggyReward.Type.SupplyLine, 1));
                // if (piggyLevel < 7) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.PiggyLevel, 1));
                // if (piggyLevel < 8) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.Hole, 1));
            // }

            public float PiggyPercent => currentMoney.amount / (float)moneyCapacity;
            public bool IsFull => currentMoney.amount >= moneyCapacity;
            public Const.Currency Percent2Money(float percent) => new Const.Currency(currentMoney.type, (int)Mathf.Lerp(0.0f, moneyCapacity, percent));
            public int RewardCount => rewards.Count;
            public bool RewardsWaiting => rewards.Count > 0;
            // public bool MaxRewardsReached => piggyLevel >= maxPiggyLevel;

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
        
        // public void GiveReward()
        // {
        //     switch (type)
        //     {
        //         case PiggyReward.Type.Coin:
        //             Wallet.Transaction(new Const.Currency(Const.CurrencyType.Coin, amount));
        //             break;
        //         case PiggyReward.Type.PiggyCoin:
        //             Wallet.Transaction(new Const.Currency(Const.CurrencyType.PiggyCoin, amount));
        //             break;
        //         case PiggyReward.Type.Ad:
        //             Wallet.Transaction(new Const.Currency(Const.CurrencyType.Ad, amount));
        //             break;
        //         case PiggyReward.Type.Shield:
        //             Warzone.THIS.GiveShield(amount);
        //             break;
        //         case PiggyReward.Type.Heart:
        //             Warzone.THIS.GiveHeart(amount);
        //             break;
        //         case PiggyReward.Type.Medkit:
        //             Warzone.THIS.GiveHeart(amount * 10);
        //             break;
        //         case PiggyReward.Type.Protection:
        //             Warzone.THIS.GiveShield(amount * 10);
        //             break;
        //         case PiggyReward.Type.MaxStack:
        //             Board.THIS.MaxStack += amount;
        //             break;
        //         // case PiggyReward.Type.SupplyLine:
        //         //     Board.THIS.SupplyLine += amount;
        //         //     break;
        //         case PiggyReward.Type.PiggyLevel:
        //             PiggyMenu.THIS.MaxPiggyLevel += amount;
        //             break;
        //         // case PiggyReward.Type.Hole:
        //         //     break;
        //     }
        // }

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
            Damage,
            Firerate,
            Splitshot,
            Weapon
        }
    } 
    #endregion
}
