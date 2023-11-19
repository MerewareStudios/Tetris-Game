using System.Text;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfferScreen : Lazyingleton<OfferScreen>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] public OfferData[] offerData;
    [Header("Visuals")]
    [SerializeField] private OfferPreview[] offerPreviews;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private TextMeshProUGUI oldText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI promotionalText;
    [SerializeField] private TextMeshProUGUI processStateText;
    [SerializeField] private GameObject loadingBar;
    [SerializeField] private GameObject buyPanel;
    [SerializeField] private Button closeButton;
    [System.NonSerialized] public float TimeScale = 1.0f;
    [System.NonSerialized] public System.Action<bool, ProcessState> OnVisibilityChanged;
    // [System.NonSerialized] private System.Action _onOfferRejected;
    // [System.NonSerialized] private System.Action _onOfferAccepted;
    [System.NonSerialized] private System.Action _onBuy;
    [TextArea] [SerializeField] private string failText;
    [TextArea] [SerializeField] private string successText;
    [TextArea] [SerializeField] private string processingText;
    
    public delegate string STR2STR(string iapID);
    public delegate decimal STR2DECIMAL(string iapID);
    public static STR2STR OnGetPriceSymbol;
    public static STR2DECIMAL OnGetPrice;
    public static System.Action<string> OnPurchaseOffer;

    [System.NonSerialized] private ProcessState _currentProcessState;
    [System.Serializable]
    public enum ProcessState
    {
        NONE,
        PROCESSING,
        SUCCESS,
        FAIL
    }
    
    // public OfferScreen Open(Type offerType, System.Action onOfferAccepted = null, System.Action onOfferRejected = null)
    public OfferScreen Open(Type offerType)
    {
        if (canvas.enabled)
        {
            return this;
        }

        // this._onOfferAccepted = onOfferAccepted;
        // this._onOfferRejected = onOfferRejected;
        
        
        canvas.enabled = true;
        canvasGroup.DOKill();
        canvasGroup.alpha = 0.0f;
        canvasGroup.DOFade(1.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true);
        
        this.gameObject.SetActive(true);
        
        
        SetupOffer(offerType);
        
        TimeScale = 0.0f;
        OnVisibilityChanged?.Invoke(true, _currentProcessState);

        return this;
    }

    public OfferScreen Close()
    {
        if (!canvas.enabled)
        {
            return this;
        }
        
        canvasGroup.DOKill();
        canvasGroup.DOFade(0.0f, 0.1f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
        {
            this.gameObject.SetActive(false);
            canvas.enabled = false;
        };        
        
        
        TimeScale = 1.0f;
        OnVisibilityChanged?.Invoke(false, _currentProcessState);
        
        return this;
    }

    public ProcessState CurrentProcessState
    {
        get => _currentProcessState;
        set
        {
            _currentProcessState = value;
            switch (value)
            {
                case ProcessState.NONE:
                    buyPanel.SetActive(true);
                    closeButton.gameObject.SetActive(true);
                    
                    loadingBar.SetActive(false);
                    
                    processStateText.gameObject.SetActive(false);
                    // processStateText.text = "";
                    break;
                case ProcessState.PROCESSING:
                    buyPanel.SetActive(false);
                    closeButton.gameObject.SetActive(false);
                    
                    loadingBar.SetActive(true);
                    
                    processStateText.gameObject.SetActive(true);
                    processStateText.text = processingText;
                    break;
                case ProcessState.SUCCESS:
                    buyPanel.SetActive(false);
                    closeButton.gameObject.SetActive(true);
                    
                    loadingBar.SetActive(false);
                    
                    processStateText.gameObject.SetActive(true);
                    processStateText.text = successText;
                    break;
                case ProcessState.FAIL:
                    buyPanel.SetActive(true);
                    closeButton.gameObject.SetActive(true);
                    
                    loadingBar.SetActive(false);
                    
                    processStateText.gameObject.SetActive(false);
                    // processStateText.text = failText;
                    break;
            }
        }
    }

    public void OnClick_Close()
    {
        Close();
        // _onOfferRejected?.Invoke();
    }
    
    public void OnClick_Buy()
    {
        _onBuy?.Invoke();
        PurchaseStarted();
    }

    private void SetupOffer(Type type)
    {
        SetupVisuals(offerData[(int)type]);
    }
    
    public void PurchaseStarted()
    {
        CurrentProcessState = ProcessState.PROCESSING;
    }
        
    public void PurchaseFinished(bool successful)
    {
        CurrentProcessState = successful ? ProcessState.SUCCESS : ProcessState.FAIL;
    }

    private void SetupVisuals(OfferData data)
    {
        _onBuy = () => OnPurchaseOffer?.Invoke(data.iapID);

        
        titleText.text = data.title;
        rewardsText.text = data.RewardInfo();
        infoText.text = data.detailedInfoStr;
        
        promotionalText.transform.parent.gameObject.SetActive(!data.promotionalText.Equals(""));
        promotionalText.text = data.promotionalText;

        string symbol = OnGetPriceSymbol.Invoke(data.iapID);
        decimal price = OnGetPrice.Invoke(data.iapID);
        
        priceText.text = symbol + price.ToString("#.00");

        if (data.oldPriceMult > 1)
        {
            oldText.gameObject.SetActive(true);
            oldText.text = symbol + (Mathf.Ceil((float)price * data.oldPriceMult) - 0.01f).ToString("#.00");
        }
        else
        {
            oldText.gameObject.SetActive(false);
        }

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

        CurrentProcessState = ProcessState.NONE;
    }
    
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }

    public static void OnPurchase(string iapID)
    {
        
    }

    [System.Serializable]
    public enum Type
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
    public class Reward
    {
        public RewardType rewardType;
        public int amount;
    }
    
    [System.Serializable]
    public class OfferData
    {
        [SerializeField] public OfferScreen.Type offerType;
        [SerializeField] public UnityEngine.Purchasing.ProductType productType;
        [SerializeField] public string iapID;
        [TextArea] [SerializeField] public string title;
        [TextArea] [SerializeField] public string detailedInfoStr;
        [SerializeField] public Reward[] rewards;
        [SerializeField] public OfferPreview.PreviewData[] previewDatas;
        [SerializeField] public int oldPriceMult = 1;
        [TextArea] [SerializeField] public string promotionalText;

        public string RewardInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < rewards.Length; i++)
            {
                Reward reward = rewards[i];

                if (reward.rewardType.Equals(RewardType.NoAds))
                {
                    continue;
                }

                stringBuilder.Append(reward.rewardType.ToTMProKey() + "+" + reward.amount);
                if (i < rewards.Length - 1)
                {
                    stringBuilder.Append("    ");
                }

            }
            
            return stringBuilder.ToString();
        }
    }

    
}
