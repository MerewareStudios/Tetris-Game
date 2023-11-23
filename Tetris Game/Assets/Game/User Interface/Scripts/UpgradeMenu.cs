using System;
using IWI;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class UpgradeMenu : Menu<UpgradeMenu>, IMenu
    {
        [Header("Purchase Options")]
        [SerializeField] private PurchaseOption[] purchaseOptions;
        [SerializeField] private RectTransform scrollRectFrame;
        [SerializeField] private RectTransform scrollPanel;
        [SerializeField] private TextMeshProUGUI maxStackText;
        [SerializeField] private TextMeshProUGUI capacityText;
        [System.NonSerialized] private bool _oneTimeDataSet = false;
        [System.NonSerialized] private int _promptIndex = -1;

        [field: System.NonSerialized] public Data SavedData { set; get; }

        public int AvailablePurchaseCount(bool updatePage)
        {
            base.TotalNotify = 0;
            for (int i = 0; i < Const.THIS.purchaseDataLookUp.Length; i++)
            {
                PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[i];
                bool hasFunds = Wallet.HasFunds(lookUp.currency);
                if (hasFunds)
                {
                    if (updatePage)
                    {
                        _promptIndex = i;
                    }

                    base.TotalNotify++;
                    // return 1;
                }
            }
            return base.TotalNotify;
        }

        public new bool Open(float duration = 0.5f)
        {
            if (base.Open(duration))
            {
                return true;
            }
            Show();
            return false;
        }

        public void OnClick_Close()
        {
            if (base.Close())
            {
                return;
            }
        }

        private void SetOneTimeData()
        {
            if (_oneTimeDataSet)
            {
                return;
            }
            _oneTimeDataSet = true;
            for (int i = 0; i < purchaseOptions.Length; i++)
            {
                PurchaseOption purchaseOption = purchaseOptions[i];
                if (!purchaseOption)
                {
                    continue;
                }
                
                purchaseOption.gameObject.SetActive(!SavedData.hiddenData[i]);

                if (!purchaseOption.gameObject.activeSelf)
                {
                    continue;
                }
                
                PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[i];

                purchaseOption
                    .SetIcon(lookUp.sprite)
                    .SetInfo(lookUp.title, lookUp.info)
                    .SetExtra(lookUp.extra);
            }
        }

        public new void Show()
        {
            base.Show();
            SetOneTimeData();
            
            for (int i = 0; i < purchaseOptions.Length; i++)
            {
                PurchaseOption purchaseOption = purchaseOptions[i];
                if (!purchaseOption)
                {
                    continue;
                }

                purchaseOption.gameObject.SetActive(!SavedData.hiddenData[i]);

                if (!purchaseOption.gameObject.activeSelf)
                {
                    continue;
                }
                
                
                PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[i];


                bool available = Wallet.HasFunds(lookUp.currency) || lookUp.currency.type.Equals(Const.CurrencyType.Ticket);

                string purchaseText = "";

                switch (lookUp.currency.type)
                {
                    case Const.CurrencyType.Coin:
                        purchaseText = "GET";
                        break;
                    case Const.CurrencyType.Gem:
                        purchaseText = "GET";
                        break;
                    case Const.CurrencyType.Ticket:
                        purchaseText = "GET";
                        break;
                    case Const.CurrencyType.Local:
                        purchaseText = "BUY";
                        break;
                }
                

                // purchaseOption.SetPrice(lookUp.currency.type.IsLocal() ? lookUp.GetLocalPrice() : CurrencyDisplay.GetCurrencyString(lookUp.currency), lookUp.currency.type, available);
                purchaseOption.SetPrice(CurrencyDisplay.GetCurrencyString(lookUp.currency), lookUp.currency.type, available);
                purchaseOption.SetPurchaseText(purchaseText);
                
                if (available)
                {
                    purchaseOption.Glimmer();
                }
            }

            maxStackText.text = Board.THIS.StackLimit.ToString();
            capacityText.text = PiggyMenu.THIS.SavedData.moneyCapacity.ToString();
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[purchaseIndex];
            
            if (!Wallet.Consume(lookUp.currency))
            {
                if (lookUp.currency.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(() =>
                    {
                        Wallet.Transaction(Const.Currency.OneAd);
                        OnClick_Purchase(purchaseIndex);
                    });
                }
                else
                {
                    purchaseOptions[purchaseIndex].PunchColor(Const.THIS.deniedFrameColor, Const.THIS.defaultFrameColor);
                    purchaseOptions[purchaseIndex].Punch(new Vector3(0.0f, -50.0f));
                }
                return;
            }
            
            purchaseOptions[purchaseIndex].PunchColor(Const.THIS.acceptedFrameColor, Const.THIS.defaultFrameColor);
            purchaseOptions[purchaseIndex].Punch(new Vector3(0.0f, 30.0f));
            purchaseOptions[purchaseIndex].Glimmer();


            OnPurchase((PurchaseType)purchaseIndex);
        }

        public void OnPurchase(PurchaseType purchaseType)
        {
            switch (purchaseType)
            {
                case PurchaseType.MAX_STACK:
                    Board.THIS._Data.maxStack++;
                    break;
                case PurchaseType.PIGGY_BANK:
                    PiggyMenu.THIS.SavedData.moneyCapacity += 5;
                    break;
                case PurchaseType.MEDKIT:
                    Warzone.THIS.Player._CurrentHealth += 20;
                    break;
            }

            if (base.Visible)
            {
                Show();
            }

            int index = (int)purchaseType;
            SavedData.instanceData[index]++;
            AnalyticsManager.PurchasedUpgrade(purchaseType.ToString(), SavedData.instanceData[index]);
        }

        // public void Prompt(int index, float amount, float duration, float delay)
        // {
        //     PurchaseOption purchaseOption = purchaseOptions[index];
        //     purchaseOption.PunchColor(Const.THIS.acceptedFrameColor, Const.THIS.defaultFrameColor);
        //     purchaseOption.PunchScale(amount, duration, delay);
        //     purchaseOption.Glimmer();
        //     // this.WaitForFrame(() =>
        //     // {
        //     //     SnapTo(purchaseOption.animationPivot);
        //     // });
        // }
        
        // public void SnapTo(RectTransform target)
        // {
        //     // Canvas.ForceUpdateCanvases();
        //     
        //     Vector2 dif = (Vector2)scrollRectFrame.transform.InverseTransformPoint(scrollPanel.position) - (Vector2)scrollRectFrame.transform.InverseTransformPoint(target.position);
        //
        //     scrollRectFrame.anchoredPosition = new Vector2(scrollRectFrame.anchoredPosition.x, dif.y);
        // }

        [Serializable]
        public enum PurchaseType
        {
            MAX_STACK,
            PIGGY_BANK,
            MEDKIT,
        }

        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool[] hiddenData;
            [SerializeField] public int[] instanceData;
            
            public Data(Data data)
            {
                hiddenData = data.hiddenData.Clone() as bool[];
                instanceData = data.instanceData.Clone() as int[];
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
        [System.Serializable]
        public class PurchaseDataLookUp
        {
            [SerializeField] public PurchaseType purchaseType;
            [SerializeField] public Const.Currency currency;
            [SerializeField] public Sprite sprite;
            [TextArea] [SerializeField] public string title;
            [TextArea] [SerializeField] public string info;
            [TextArea] [SerializeField] public string extra;

            // public string GetLocalPrice()
            // {
            //     return "";
            // }
        }
    }
}