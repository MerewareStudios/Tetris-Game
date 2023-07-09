using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PiggyMenu : Menu<PiggyMenu>, IMenu
{
    [Header("End Level Screen")]
    [Header("Bars")]
    [SerializeField] private MarkedProgress _markedProgressPiggy;
    [Header("Buttons")]
    [SerializeField] private RewardButton[] piggyRewardButtons;
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI pigLevelText;
    [SerializeField] private TextMeshProUGUI moneyCountText;
    [SerializeField] private TextMeshProUGUI investmentAmountText;
    [SerializeField] private TextMeshProUGUI freeInvestmentAmountText;
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
        set => _Data.maxPiggyLevel = value;
        get => _Data.maxPiggyLevel;
    }
    private int MoneyCount
    {
        set => moneyCountText.text = value.CoinAmount();
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

        List<Vector3> positions = CircleLayoutGroup.GetPoints(_rewardsCenter.position, _Data.RewardCount,  _Data.RewardCount.Direction(), _Data.RewardCount.Radius());

        for (int i = 0; i < _Data.RewardCount; i++)
        {
            RewardButton rewardButton = piggyRewardButtons[i];
            PiggyReward piggyReward = _Data.rewards[i];
            rewardButton.OnClick(() =>
            {
                _Data.rewards.Remove(piggyReward);
                rewardButton.ShowReward(piggyReward);

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
    private void InvestCoins(int investAmount, float delay = 0.25f)
    {
        ShowScreen(false, true, false);
        ShowPiggyScreenButtons(false, false);
        
        DOVirtual.DelayedCall(delay, () =>
        {
            InvestAnimation(investAmount);
        });
    }
    private void InvestAnimation(int amount)
    {
        Wallet.COIN.Transaction(-amount);
        int maxCoin = 6;
        
        int count = Mathf.Min(amount, maxCoin);
        float posDif = 0.3f;
            
        Vector3 screenStart = Wallet.COIN.iconPivot.position;
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
                    AddMoney(amount, 0.0f);
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
    }
    private void Show()
    {
        Time.timeScale = 0.0f;

        UIManager.THIS.ScaleTransactors(1.5f, true);

        _Data = _data;
        
        bool investForFree = Wallet.COIN.Amount == 0;
        
        ShowScreen(true, false, false);
        ShowOptionButtons(true, !_Data.RewardsWaiting && !investForFree && !_Data.MaxRewardsReached, !_Data.RewardsWaiting && investForFree && !_Data.MaxRewardsReached, _Data.RewardsWaiting, _Data.MaxRewardsReached);
    }
    public void Hide()
    {
        Time.timeScale = 1.0f;
        piggyJumpAnimation.DOKill();
        
        UIManager.THIS.ScaleTransactors(1.0f);
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
        keepButton.SetActive(keepState);
        investButton.SetActive(investState);
        if (investState)
        {
            investmentAmountText.text = InvestWithStrategy(Wallet.COIN.Amount).CoinAmount();
        }
        investFreeButton.SetActive(investFreeState);
        if (investFreeState)
        {
            freeInvestmentAmountText.text = _Data.freeInvestmentAmount.CoinAmount();
        }
        justOpenButton.SetActive(justOpenState);
        openRewardsButton.SetActive(rewardState);
    }
    private int InvestWithStrategy(int amount)
    {
        if (amount <= 3)
        {
            return amount;
        }
        amount = Mathf.FloorToInt(amount * 0.5f);
        if (amount % 2 == 1)
        {
            amount++;
        }

        amount = Mathf.Clamp(amount, 1, 50);
        return amount;
    }
    public void Option_Keep()
    {
        CloseAction();
    }
    public void Option_Invest()
    {
        InvestCoins(InvestWithStrategy(Wallet.COIN.Amount));
    }
    
    public void Option_InvestFree()
    {
        Debug.LogWarning("Watch Ad - Not Implemented, invest");
        InvestCoins(_Data.freeInvestmentAmount);
    }
    public void Option_JustOpen()
    {
        OnClick_Break();
        // ShowScreen(false, true, false);
        // ShowPiggyScreenButtons(_PiggyData.piggyLevel > 0, true);
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
            [SerializeField] public int freeInvestmentAmount = 25;
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
                this.freeInvestmentAmount = data.freeInvestmentAmount;
                this.rewards.CopyFrom(data.rewards);
            }

            
            public void GenerateRewards()
            {
                for (int i = 0; i < piggyLevel; i++)
                {
                    rewards.Add(new PiggyReward());
                }
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
        

        public object Clone()
        {
            return new PiggyReward(this);
        }

        public enum Type
        {
            Coins,
            Gems,
            Shield,
            Heart,
            Splitshot,
            Damage,
            Firerate,
            Agility,
            FreeUpgrade,
            Block,
            Luck,
            MaxStack,
        }
    } 
    #endregion
}
