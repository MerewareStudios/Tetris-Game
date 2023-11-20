using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferScreen : Lazyingleton<OfferScreen>
{
    [System.NonSerialized] private Data _data;
    [System.NonSerialized] private OfferData _currentOfferData;
    [System.NonSerialized] private bool Active = false;
    [System.NonSerialized] private ProcessState _currentProcessState;
    [System.NonSerialized] public float TimeScale = 1.0f;
    [System.NonSerialized] public System.Action<bool, ProcessState> OnVisibilityChanged;
    [System.NonSerialized] private System.Action _onBuy;
    [System.NonSerialized] private Tween _delayedCheck = null;

    [Header("Offer Data")]
    [SerializeField] public OfferData[] offerData;
    [SerializeField] private OfferPreview[] offerPreviews;
    // [TextArea] [SerializeField] private string failText;
    [TextArea] [SerializeField] private string successText;
    [TextArea] [SerializeField] private string processingText;
    [Header("Menu")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image offerGrid;
    [SerializeField] private Vector2 offerGridOpenSize;
    [SerializeField] private Vector2 offerGridCloseSize;
    [Header("Visuals")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI oldText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI promotionalText;
    [SerializeField] private TextMeshProUGUI processStateText;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject unpackPanel;
    [SerializeField] private Button closeButton;
    
    public delegate string STR2STR(string iapID);
    public delegate decimal STR2DECIMAL(string iapID);
    public delegate void UNPACK(Reward[] rewards, System.Action onFinish);
    public delegate bool CONDITIONAL();
    public static STR2STR OnGetPriceSymbol;
    public static STR2DECIMAL OnGetPrice;
    public static System.Action<string> OnPurchaseOffer;
    public static UNPACK OnReward;
    public CONDITIONAL SkipCondition;

    
    public Data _Data
    {
        set
        {
            this._data = value;
            //open screen to give rewards
        }
        get => _data;
    }
    public ProcessState CurrentProcessState
    {
        get => _currentProcessState;
        set
        {
            _currentProcessState = value;
            
            fadeCanvasGroup.DOKill();
            fadeCanvasGroup.DOFade(1.0f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
            
            offerGrid.rectTransform.DOKill();
            offerGrid.rectTransform.DOSizeDelta(offerGridOpenSize, 0.2f).SetEase(Ease.OutQuint).SetUpdate(true);
            
            switch (value)
            {
                case ProcessState.NONE:
                    buyPanel.SetActive(true);
                    closeButton.gameObject.SetActive(true);
                    loadingBar.SetActive(false);
                    processStateText.gameObject.SetActive(false);
                    unpackPanel.SetActive(false);
                    break;
                case ProcessState.PROCESSING:
                    buyPanel.SetActive(false);
                    closeButton.gameObject.SetActive(false);
                    loadingBar.SetActive(true);
                    processStateText.gameObject.SetActive(true);
                    processStateText.text = processingText;
                    unpackPanel.SetActive(false);
                    break;
                case ProcessState.SUCCESS:
                    buyPanel.SetActive(false);
                    closeButton.gameObject.SetActive(false);
                    loadingBar.SetActive(false);
                    processStateText.gameObject.SetActive(true);
                    processStateText.text = successText;
                    unpackPanel.SetActive(true);
                    break;
                case ProcessState.FAIL:
                    buyPanel.SetActive(true);
                    closeButton.gameObject.SetActive(true);
                    loadingBar.SetActive(false);
                    processStateText.gameObject.SetActive(false);
                    unpackPanel.SetActive(false);
                    break;
            }
        }
    }
    
    
    public void Open(OfferType offerType, Mode mode = Mode.Offer)
    {
        if (SkipCondition != null && SkipCondition.Invoke())
        {
            CheckForUnpack(5.0f);
            return;
        }
        
        this._currentOfferData = offerData[(int)offerType];
        SetupVisuals(_currentOfferData, mode);
        if (Active)
        {
            return;
        }

        Active = true;


        this.gameObject.SetActive(true);
        canvas.enabled = true;
        
        canvasGroup.DOKill();
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
        
        
        TimeScale = 0.0f;
        OnVisibilityChanged?.Invoke(true, _currentProcessState);
    }
    private void Close()
    {
        if (!Active)
        {
            return;
        }

        Active = false;
        
        canvasGroup.DOKill();
        canvasGroup.DOFade(0.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            this.gameObject.SetActive(false);
            canvas.enabled = false;

            CheckForUnpack(2.5f);
        };        
        
        
        TimeScale = 1.0f;
        OnVisibilityChanged?.Invoke(false, _currentProcessState);
    }
    private void SetupVisuals(OfferData data, Mode mode = Mode.Offer)
    {
        for (int i = 0; i < offerPreviews.Length; i++)
        {
            bool iconEnabled = i < data.previewDatas.Length;
            offerPreviews[i].gameObject.SetActive(iconEnabled);

            bool plusEnabled = iconEnabled && (i != data.previewDatas.Length - 1);
            offerPreviews[i].SetPlusState(plusEnabled);

            if (iconEnabled)
            {
                offerPreviews[i].Set(data.previewDatas[i]);
            }
        }

        switch (mode)
        {
            case Mode.Offer:
                infoPanel.SetActive(true);
                
                _onBuy = () => OnPurchaseOffer?.Invoke(data.iapID);

        
                titleText.text = data.title;
                // rewardsText.text = data.RewardInfo();
                infoText.text = data.detailedInfoStr;
        
                promotionalText.transform.parent.gameObject.SetActive(!data.promotionalText.Equals(""));
                promotionalText.text = data.promotionalText;

                string symbol = OnGetPriceSymbol.Invoke(data.iapID);
                decimal price = OnGetPrice.Invoke(data.iapID);
        
                priceText.text = symbol + price.ToString("#.00");

                oldText.gameObject.SetActive(data.oldPriceMult > 1);

                if (data.oldPriceMult > 1)
                {
                    oldText.text = symbol + (Mathf.Ceil((float)price * data.oldPriceMult) - 0.01f).ToString("#.00");
                }
                else
                {
                    oldText.gameObject.SetActive(false);
                }

                offerGrid.rectTransform.sizeDelta = offerGridOpenSize;

                CurrentProcessState = ProcessState.NONE;
                break;
            case Mode.Unpack:
                infoPanel.SetActive(false);
                
                CurrentProcessState = ProcessState.SUCCESS;
                break;
        }
    }

#region OnClick
    public void OnClick_Close()
    {
        Close();
    }
    public void OnClick_Buy()
    {
        _onBuy?.Invoke();
        PurchaseStarted();
    }
    public void OnClick_Unpack()
    {
        UnpackReward(_currentOfferData.offerType);
    }
#endregion
#region Purchase
    private void PurchaseStarted()
    {
        CurrentProcessState = ProcessState.PROCESSING;
    }
        
    public void OnPurchaseComplete(string iapID, bool successful)
    {
        if (!successful)
        {
            CurrentProcessState = ProcessState.FAIL;
            return;
        }
        OfferData offerDat = ID2OfferData(iapID);
        if (offerDat == null)
        {
            Debug.LogError("Could not find purchase iapID.");
            return;
        }
        
        _Data.offers.Add(offerDat.offerType);
        
        if (!Active || (CurrentProcessState.Equals(ProcessState.PROCESSING) && _currentOfferData.offerType.Equals(offerDat.offerType)))
        {
            ShowNextUnpackOrClose();
        }
    }
    private void UnpackReward(OfferType offerType)
    {
        if (OnReward == null)
        {
            return;
        }
        
        unpackPanel.SetActive(false);

        offerGrid.rectTransform.DOKill();
        offerGrid.rectTransform.DOSizeDelta(offerGridCloseSize, 0.25f).SetEase(Ease.OutQuint).SetUpdate(true);
        
        fadeCanvasGroup.DOKill();
        fadeCanvasGroup.DOFade(0.0f, 0.2f).SetEase(Ease.InOutSine).SetUpdate(true);
        
        OnReward.Invoke(offerData[(int)offerType].rewards, ShowNextUnpackOrClose);
        _Data.offers.Remove(offerType);
    }

    private void ShowNextUnpackOrClose()
    {
        if (_Data.offers.Count == 0)
        {
            Close();
            return;
        }
        Open(_Data.offers.Last(), Mode.Unpack);
    }

    public void CheckForUnpack(float delay)
    {
        _delayedCheck?.Kill();
        _delayedCheck = DOVirtual.DelayedCall(delay, () =>
        {
            if (_Data.offers.Count == 0)
            {
                return;
            }

            if (Active)
            {
                return;
            }
            Open(_Data.offers.Last(), Mode.Unpack);
        });
    }
#endregion
#region Conversions
    private OfferData ID2OfferData(string iapID)
    {
        return offerData.FirstOrDefault(data => data.iapID.Equals(iapID));
    }

    public Vector3 PreviewScreenPosition(int rewardIndex)
    {
        return offerPreviews[rewardIndex].transform.position;
    }
#endregion
#region Mono
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }
#endregion
#region Data
    [System.Serializable]
    public enum ProcessState
    {
        NONE,
        PROCESSING,
        SUCCESS,
        FAIL,
    }
    [System.Serializable]
    public enum OfferType
    {
        RemoveAds,
        CoinPack,
        PiggyCoinPack,
        TicketPack,
        HealthPack,
        OfferPack1,
        OfferPack2,
        OfferPack3,
        OfferPack4,
        OfferPack5,
    }
    [System.Serializable]
    public enum RewardType
    {
        NoAds,
        Coin,
        PiggyCoin,
        Ticket,
        Heart,
    }
    [System.Serializable]
    public enum Mode
    {
        Offer,
        Unpack,
    }
    [System.Serializable]
    public class Reward
    {
        public RewardType rewardType;
        public int amount;
    }
    [System.Serializable]
    public class OfferData
    {
        [SerializeField] public OfferScreen.OfferType offerType;
        [SerializeField] public UnityEngine.Purchasing.ProductType productType;
        [SerializeField] public string iapID;
        [TextArea] [SerializeField] public string title;
        [TextArea] [SerializeField] public string detailedInfoStr;
        [SerializeField] public Reward[] rewards;
        [SerializeField] public OfferPreview.PreviewData[] previewDatas;
        [SerializeField] public int oldPriceMult = 1;
        [TextArea] [SerializeField] public string promotionalText;

        // public string RewardInfo()
        // {
        //     StringBuilder stringBuilder = new StringBuilder();
        //
        //     for (int i = 0; i < rewards.Length; i++)
        //     {
        //         Reward reward = rewards[i];
        //
        //         if (reward.rewardType.Equals(RewardType.NoAds))
        //         {
        //             continue;
        //         }
        //
        //         stringBuilder.Append(reward.rewardType.ToTMProKey() + "+" + reward.amount);
        //         if (i < rewards.Length - 1)
        //         {
        //             stringBuilder.Append("    ");
        //         }
        //
        //     }
        //     
        //     return stringBuilder.ToString();
        // }
    }
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public List<OfferType> offers;
            
        public Data(Data data)
        {
            offers = new List<OfferType>(data.offers);
        }

        public object Clone()
        {
            return new Data(this);
        }
    } 
#endregion
}
