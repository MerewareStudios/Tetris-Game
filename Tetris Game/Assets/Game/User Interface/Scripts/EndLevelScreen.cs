using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EndLevelScreen : Menu<EndLevelScreen>, IMenu
{
    [Header("End Level Screen")]
    [Header("Bars")]
    [SerializeField] private MarkedProgress _markedProgressPiggy;
    [Header("Images")]
    [SerializeField] private Image adInvestIcon;
    [SerializeField] private RectTransform piggyJumpPivot;
    [Header("Buttons")]
    [SerializeField] private Button breakButton;
    [SerializeField] private Button continueButton;
    [Header("Tabs")]
    [SerializeField] private GameObject piggyBankParent;
    [SerializeField] private GameObject optionsParent;
    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI moneyInvestText;
    [SerializeField] private TextMeshProUGUI pigLevelText;
    [SerializeField] private TextMeshProUGUI moneyCountText;
    [Header("Pivots")]
    [SerializeField] private RectTransform _rectTransformPiggyIcon;
    [SerializeField] private RectTransform _coinTarget;
    
    [System.NonSerialized] private PiggyData _piggyData;


    public PiggyData _PiggyData
    {
        set
        {
            _piggyData = value;
            
            PiggyLevel = value.piggyLevel;
            _markedProgressPiggy._Progress = value.PiggyPercent;
            MoneyCount = value.moneyCurrent;
        }
        get => _piggyData;
    }

    public new bool Open(float duration = 0.5f)
    {
        if (base.Open(duration))
        {
            return true;
        }
        Time.timeScale = 0.0f;
        Show();

        piggyJumpPivot.DOKill();
        piggyJumpPivot.localPosition = Vector3.one;
        piggyJumpPivot.DOPunchPosition(new Vector3(0.0f, 12.0f, 0.0f), 0.6f, 6, 0.296f).SetLoops(-1, LoopType.Restart).SetEase(Ease.OutQuint).SetUpdate(true);
        
        return false;
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
    private void Invest(int amount)
    {
        MoneyTransactor.THIS.Transaction(-amount);
        int maxCoin = 6;
        
        int count = Mathf.Min(amount, maxCoin);
        float posDif = 0.3f;
            
        Vector3 screenStart = MoneyTransactor.THIS.IconPivot.position;
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
    public new void Close()
    {
        if (base.Close())
        {
            return;
        }
        piggyJumpPivot.DOKill();

        Time.timeScale = 1.0f;
        MoneyTransactor.THIS.Scale(1.0f);
    }
    private void Show()
    {
        MoneyTransactor.THIS.Scale(2.0f);

        _PiggyData = _piggyData;
        
        optionsParent.SetActive(true);
        piggyBankParent.SetActive(false);
        
        breakButton.gameObject.SetActive(false);
        continueButton.gameObject.SetActive(false);
        
        bool investForFree = MoneyTransactor.THIS.Amount == 0;
        
        adInvestIcon.enabled = investForFree;
        moneyInvestText.text = (investForFree ? 50 : InvestWithStrategy(MoneyTransactor.THIS.Amount)).CoinAmount();
    }

    public void OnClick_Break()
    {
        
    }
    public void OnClick_Continue()
    {
        Close();
    }
    
    public void OnClick_PiggyBank()
    {
        bool investForFree = MoneyTransactor.THIS.Amount == 0;
        int finalInvestment = investForFree ? 150 : InvestWithStrategy(MoneyTransactor.THIS.Amount);
        
        if (investForFree)
        {
            Debug.LogWarning("Watch Ad - Not Implemented, invest");
            PiggyBankAction();
        }
        else
        {
            PiggyBankAction();
        }

        void PiggyBankAction()
        {
            optionsParent.SetActive(false);
            piggyBankParent.SetActive(true);

            DOVirtual.DelayedCall(0.25f, () =>
            {
                Invest(finalInvestment);
            });
        }
    }
    

    #region Adds


    public void AddMoney(int count, float delay = 0.0f)
    {
        _PiggyData.moneyCurrent += count;
        int excess = Mathf.Clamp(_PiggyData.moneyCurrent - _PiggyData.moneyCapacity, 0, int.MaxValue);

        float percentChange = count / 100.0f;

        _PiggyData.moneyCurrent = Mathf.Clamp(_PiggyData.moneyCurrent, 0, _PiggyData.moneyCapacity);
        _markedProgressPiggy.ProgressAnimated(_piggyData.PiggyPercent, Mathf.Clamp(percentChange, 0.2f, 0.8f), delay, Ease.OutQuad, 
            
(value) => MoneyCount = _PiggyData.Percent2Money(value) , 
            
    () =>
        {
            if (_PiggyData.IsFull)
            {
                PiggyLevel++;

                _PiggyData.moneyCurrent = 0;

                //only return to zero
                _markedProgressPiggy.ProgressAnimated(_piggyData.PiggyPercent, 0.2f, 0.25f, Ease.Linear,null, () =>
                {
                    MoneyCount = 0;
                    if (excess > 0)
                    {
                        AddMoney(excess, 0.15f);
                    }
                    else
                    {
                        ShowPiggyOptions();
                    }

                });
            }
            else
            {
                ShowPiggyOptions();
            }
        });
    }

    private void ShowPiggyOptions()
    {
        if (_PiggyData.piggyLevel > 0)
        {
            breakButton.gameObject.SetActive(true);
        }
        continueButton.gameObject.SetActive(true);
    }

    private int PiggyLevel
    {
        set
        {
            _PiggyData.piggyLevel = value;
            pigLevelText.text = _piggyData.piggyLevel.ToString();

            RectTransform rectTransformPiggyCount = pigLevelText.rectTransform;
            
            rectTransformPiggyCount.DOKill();
            rectTransformPiggyCount.localScale = Vector3.one;
            rectTransformPiggyCount.DOPunchScale(Vector3.one * 0.5f, 0.25f, 1).SetUpdate(true);
            
            PunchPiggyIcon();
        }
        get => _PiggyData.piggyLevel;
    }

    private void PunchPiggyIcon(float amount = 0.25f)
    {
        _rectTransformPiggyIcon.DOKill();
        _rectTransformPiggyIcon.localScale = Vector3.one;
        _rectTransformPiggyIcon.localPosition = Vector3.zero;
        _rectTransformPiggyIcon.DOPunchScale(Vector3.one * amount, 0.75f, 1).SetUpdate(true);
    }
    private int MoneyCount
    {
        set => moneyCountText.text = value.CoinAmount();
        get => _PiggyData.moneyCurrent;
    }

    #endregion
    
    [System.Serializable]
    public class PiggyData : ICloneable
    {
        [SerializeField] public int piggyLevel;
        [SerializeField] public int moneyCurrent;
        [SerializeField] public int moneyCapacity;

        public PiggyData()
        {
                
        }
        public PiggyData(PiggyData piggyData)
        {
            this.piggyLevel = piggyData.piggyLevel;
            this.moneyCurrent = piggyData.moneyCurrent;
            this.moneyCapacity = piggyData.moneyCapacity;
        }

        public float PiggyPercent => moneyCurrent / (float)moneyCapacity;
        public bool IsFull => moneyCurrent >= moneyCapacity;
        
        public int Percent2Money(float percent) => (int)Mathf.Lerp(0.0f, moneyCapacity, percent);

        public object Clone()
        {
            return new PiggyData(this);
        }
    } 
}
