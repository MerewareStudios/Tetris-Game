using System;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using Game.UI;
using Internal.Core;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class PiggyMenu : Menu<PiggyMenu>, IMenu
{
    [Header("End Level Screen")]
    [Header("Pivots")]
    [SerializeField] private RectTransform leftPivot;
    [SerializeField] private RectTransform rightPivot;
    [Header("Bars")]
    [SerializeField] private MarkedProgress _markedProgressPiggy;
    [Header("Buttons")]
    [SerializeField] private RewardButton[] piggyRewardButtons;
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI pigLevelText;
    [Header("Curreny Displays")]
    [SerializeField] private CurrencyDisplay piggyCurrencyDisplay;
    [SerializeField] private CurrencyDisplay investmentCurrencyDisplay;
    [SerializeField] private CurrencyDisplay freeInvestmentCurrencyDisplay;
    [Header("Pivots")]
    [SerializeField] private RectTransform _rectTransformPiggyIcon;
    [SerializeField] private RectTransform _coinTarget;
    [SerializeField] private RectTransform _rewardsCenter;
    [Header("Animation")]
    [SerializeField] private DOTweenAnimation piggyJumpAnimation;
    
    [System.NonSerialized] private Data _data;


    #region Fields
    public Data _Data
    {
        set
        {
            _data = value;
            
            PiggyLevel = value.piggyLevel;
            _markedProgressPiggy._Progress = value.PiggyPercent;
            MoneyCount = value.moneyCurrent;
        }
        get => _data;
    }
    private int PiggyLevel
    {
        set
        {
            _Data.piggyLevel = Mathf.Clamp(value, 0, _Data.maxPiggyLevel);
            pigLevelText.text = (_Data.piggyLevel == _Data.maxPiggyLevel) ? "MAX" : _data.piggyLevel.ToString();

            RectTransform rectTransformPiggyCount = pigLevelText.rectTransform;
            
            rectTransformPiggyCount.DOKill();
            rectTransformPiggyCount.localScale = Vector3.one;
            rectTransformPiggyCount.DOPunchScale(Vector3.one * 0.5f, 0.25f, 1).SetUpdate(true);
            
            PunchPiggyIcon();
        }
        get => _Data.piggyLevel;
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

    private int MoneyCount
    {
        set => piggyCurrencyDisplay.Display(Const.CurrencyType.Coin, value);
        get => _Data.moneyCurrent;
    }
    #endregion
    #region Animations

    private void PunchPiggyIcon(float amount = 0.25f)
    {
        _rectTransformPiggyIcon.DOKill();
        _rectTransformPiggyIcon.localScale = Vector3.one;
        _rectTransformPiggyIcon.localPosition = Vector3.zero;
        _rectTransformPiggyIcon.DOPunchScale(Vector3.one * amount, 0.75f, 1).SetUpdate(true);
    }

    #endregion
    #region Rewards
    private void DisplayRewards()
    {
        ShowScreen(false, false, true);
        foreach (var button in piggyRewardButtons)
        {
            button.gameObject.SetActive(false);
        }
        
        _Data.rewards.Shuffle();

        List<Vector3> positions = CircleLayoutGroup.GetPoints(_rewardsCenter.position, _Data.RewardCount,  _Data.RewardCount.Direction(), _Data.RewardCount.Radius());

        for (int i = 0; i < _Data.RewardCount; i++)
        {
            RewardButton rewardButton = piggyRewardButtons[i];
            PiggyReward piggyReward = _Data.rewards[i];
            rewardButton.OnClick(() =>
            {
                _Data.rewards.Remove(piggyReward);
                rewardButton.ShowReward(piggyReward);

                if (_Data.rewards.Count <= 0)
                {
                    DOVirtual.DelayedCall(1.25f, CloseAction).SetUpdate(true);
                }

            }).Show(_rewardsCenter.position, positions[i], i * 0.1f);
        }
    }

    
    #endregion
    #region Invest
    private void AddMoney(int count, float delay = 0.0f)
    {
        _Data.moneyCurrent += count;
        int excess = Mathf.Clamp(_Data.moneyCurrent - _Data.moneyCapacity, 0, int.MaxValue);

        float percentChange = count / 100.0f;

        _Data.moneyCurrent = Mathf.Clamp(_Data.moneyCurrent, 0, _Data.moneyCapacity);
        _markedProgressPiggy.ProgressAnimated(_data.PiggyPercent, Mathf.Clamp(percentChange, 0.2f, 0.8f), delay, Ease.OutQuad, 
            
            (value) => MoneyCount = _Data.Percent2Money(value) , 
            
            () =>
            {
                if (_Data.IsFull)
                {
                    PiggyLevel++;

                    _Data.moneyCurrent = 0;

                    _markedProgressPiggy.ProgressAnimated(_data.PiggyPercent, 0.2f, 0.25f, Ease.Linear,null, () =>
                    {
                        MoneyCount = 0;
                        if (excess > 0)
                        {
                            AddMoney(excess, 0.15f);
                        }
                        else
                        {
                            ShowPiggyScreenButtons(_Data.piggyLevel > 0, true);
                        }

                    });
                }
                else
                {
                    ShowPiggyScreenButtons(_Data.piggyLevel > 0, true);
                }
            });
    }
    private void InvestCoins(Const.Currency currency)
    {
        ShowScreen(false, true, false);
        ShowPiggyScreenButtons(false, false);
        
        DOVirtual.DelayedCall(0.4f, () =>
        {
            InvestAnimation(currency);
        });
    }
    private void InvestAnimation(Const.Currency currency)
    {
        Wallet.Transaction(currency);
        int maxCoin = 6;
        
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
                PunchPiggyIcon(0.4f);
                    
                if (index == count - 1)
                {
                    AddMoney(currency.amount, 0.0f);
                }

            })).SetDelay(i * 0.125f);
        }
    }
    #endregion
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
        Hide();
        return false;
    }
    private void CloseAction()
    {
        this.Close();
        LevelManager.THIS.LoadLevel();
    }
    private void Show()
    {
        UIManager.MenuMode(true);

        Wallet.ScaleTransactors(1.5f, true);

        _Data = _data;
        
        bool investForFree = Wallet.COIN.Amount == 0;
        
        ShowScreen(true, false, false);
        ShowOptionButtons(true, !_Data.RewardsWaiting && !investForFree && !_Data.MaxRewardsReached, !_Data.RewardsWaiting && investForFree && !_Data.MaxRewardsReached, _Data.RewardsWaiting, _Data.MaxRewardsReached);
    }
    public void Hide()
    {
        UIManager.MenuMode(false);
        piggyJumpAnimation.DOKill();
        
        Wallet.ScaleTransactors(1.0f);
    }
    #endregion
    #region Option Buttons
    [Header("Option Buttons")]
    [SerializeField] private GameObject keepButton;
    [SerializeField] private GameObject investButton;
    [SerializeField] private GameObject investFreeButton;
    [SerializeField] private GameObject justOpenButton;
    [SerializeField] private GameObject openRewardsButton;

    public void ShowOptionButtons(bool keepState, bool investState, bool investFreeState, bool rewardState, bool justOpenState)
    {
        leftPivot.DOKill();
        leftPivot.localRotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
        leftPivot.DOLocalRotate(Vector3.zero, 0.4f).SetDelay(0.1f).SetEase(Ease.OutBack, 0.5f).SetUpdate(true);
        
        rightPivot.DOKill();
        rightPivot.localRotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
        rightPivot.DOLocalRotate(Vector3.zero, 0.4f).SetDelay(0.4f).SetEase(Ease.OutBack, 0.5f).SetUpdate(true);
        
        keepButton.SetActive(keepState);
        investButton.SetActive(investState);
        if (investState)
        {
            Const.Currency investmentAmount = InvestWithStrategy(Wallet.COIN.Currency);
            investmentCurrencyDisplay.Display(investmentAmount);
        }
        investFreeButton.SetActive(investFreeState);
        if (investFreeState)
        {
            freeInvestmentCurrencyDisplay.Display(_Data.freeInvestment);
        }
        justOpenButton.SetActive(justOpenState);
        openRewardsButton.SetActive(rewardState);
    }
    private Const.Currency InvestWithStrategy(Const.Currency currency)
    {
        if (currency.amount <= 3)
        {
            return currency;
        }
        currency.amount = Mathf.FloorToInt(currency.amount * 0.5f);
        if (currency.amount % 2 == 1)
        {
            currency.amount++;
        }

        currency.amount = Mathf.Clamp(currency.amount, 1, 50);
        return currency;
    }
    public void Option_Keep()
    {
        CloseAction();
    }
    public void Option_Invest()
    {
        InvestCoins(InvestWithStrategy(Wallet.COIN.Currency));
    }
    
    public void Option_InvestFree()
    {
        Debug.LogWarning("Watch Ad - Not Implemented, invest");
        InvestCoins(_Data.freeInvestment);
    }
    public void Option_JustOpen()
    {
        OnClick_Break();
    }
    public void Option_OpenRewards()
    {
        DisplayRewards();
    }
    #endregion
    #region Screens
    [Header("Screens")]
    [SerializeField] private GameObject optionsScreen;
    [SerializeField] private GameObject piggyBankScreen;
    [SerializeField] private GameObject rewardsScreen;

    public void ShowScreen(bool optionsState, bool piggyState, bool rewardState)
    {
        optionsScreen.SetActive(optionsState);
        piggyBankScreen.SetActive(piggyState);

        
        if (piggyState)
        {
            piggyJumpAnimation.CreateTween(false, true);
            
            piggyBankScreen.transform.DOKill();
            piggyBankScreen.transform.localPosition = Vector3.down * 2000.0f;
            piggyBankScreen.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        else
        {
            piggyJumpAnimation.DOKill();
        }
        rewardsScreen.SetActive(rewardState);
    }
    #endregion
    #region Piggy Screen Buttons
    [Header("Piggy Screen Buttons")]
    [SerializeField] private GameObject breakButton;
    [SerializeField] private GameObject continueButton;

    public void ShowPiggyScreenButtons(bool breakState, bool continueState)
    {
        breakButton.SetActive(breakState);
        continueButton.SetActive(continueState);
    }
    public void OnClick_Continue()
    {
        CloseAction();
    }
    public void OnClick_Break()
    {
        _Data.GenerateRewards();
        DisplayRewards();
    }
    #endregion
    #region Classes
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public int piggyLevel;
            [SerializeField] public int maxPiggyLevel = 9;
            [SerializeField] public int moneyCurrent;
            [SerializeField] public int moneyCapacity;
            [SerializeField] public Const.Currency freeInvestment;
            [SerializeField] public List<PiggyReward> rewards = new();

            public Data()
            {
                    
            }
            
            public Data(Data data)
            {
                this.piggyLevel = data.piggyLevel;
                this.maxPiggyLevel = data.maxPiggyLevel;
                this.moneyCurrent = data.moneyCurrent;
                this.moneyCapacity = data.moneyCapacity;
                this.freeInvestment = data.freeInvestment;
                this.rewards.CopyFrom(data.rewards);
            }

            
            public void GenerateRewards()
            {
                if (piggyLevel < 1) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.Gem, UnityEngine.Random.Range(5, 26)));
                if (piggyLevel < 2) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.Coin, UnityEngine.Random.Range(5, 26)));
                if (piggyLevel < 3) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.Shield, UnityEngine.Random.Range(5, 16)));
                if (piggyLevel < 4) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.Heart, UnityEngine.Random.Range(5, 11)));
                if (piggyLevel < 5) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.Ad, UnityEngine.Random.Range(1, 3)));
                if (piggyLevel < 6) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.MaxStack, 1));
                // if (piggyLevel < 7) return;
                // rewards.Add(new PiggyReward(PiggyReward.Type.SupplyLine, 1));
                if (piggyLevel < 7) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.PiggyLevel, 1));
                if (piggyLevel < 8) return;
                rewards.Add(new PiggyReward(PiggyReward.Type.Hole, 1));
            }

            public float PiggyPercent => moneyCurrent / (float)moneyCapacity;
            public bool IsFull => moneyCurrent >= moneyCapacity;
            public int Percent2Money(float percent) => (int)Mathf.Lerp(0.0f, moneyCapacity, percent);
            public int RewardCount => rewards.Count;
            public bool RewardsWaiting => rewards.Count > 0;
            public bool MaxRewardsReached => piggyLevel >= maxPiggyLevel;

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
        
        public void GiveReward()
        {
            switch (type)
            {
                case PiggyReward.Type.Coin:
                    Wallet.Transaction(new Const.Currency(Const.CurrencyType.Coin, amount));
                    break;
                case PiggyReward.Type.Gem:
                    Wallet.Transaction(new Const.Currency(Const.CurrencyType.PiggyCoin, amount));
                    break;
                case PiggyReward.Type.Ad:
                    Wallet.Transaction(new Const.Currency(Const.CurrencyType.Ad, amount));
                    break;
                case PiggyReward.Type.Shield:
                    Warzone.THIS.GiveShield(amount);
                    break;
                case PiggyReward.Type.Heart:
                    Warzone.THIS.GiveHeart(amount);
                    break;
                case PiggyReward.Type.Medkit:
                    Warzone.THIS.GiveHeart(amount * 10);
                    break;
                case PiggyReward.Type.Protection:
                    Warzone.THIS.GiveShield(amount * 10);
                    break;
                case PiggyReward.Type.MaxStack:
                    Board.THIS.MaxStack += amount;
                    break;
                // case PiggyReward.Type.SupplyLine:
                //     Board.THIS.SupplyLine += amount;
                //     break;
                case PiggyReward.Type.PiggyLevel:
                    PiggyMenu.THIS.MaxPiggyLevel += amount;
                    break;
                case PiggyReward.Type.Hole:
                    break;
            }
        }

        public object Clone()
        {
            return new PiggyReward(this);
        }

        public enum Type
        {
            Coin,
            Gem,
            Ad,
            Shield,
            Heart,
            Medkit,
            Protection,
            MaxStack,
            // SupplyLine,
            PiggyLevel,
            Hole,
        }
    } 
    #endregion
}
