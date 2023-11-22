using System;
using Internal.Core;
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
        [System.NonSerialized] private Data _data;
        [System.NonSerialized] private bool oneTimeDataSet = false;

        public Data _Data
        {
            set => _data = value;
            get => _data;
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
            if (oneTimeDataSet)
            {
                return;
            }
            oneTimeDataSet = true;
            for (int i = 0; i < purchaseOptions.Length; i++)
            {
                PurchaseOption purchaseOption = purchaseOptions[i];
                if (!purchaseOption)
                {
                    continue;
                }
                
                purchaseOption.gameObject.SetActive(!_Data.hiddenData[i]);

                if (!purchaseOption.gameObject.activeSelf)
                {
                    continue;
                }
                
                PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[i];


                
                purchaseOption
                    .SetIcon(lookUp.sprite)
                    // .SetBestBadge(lookUp.best)
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

                purchaseOption.gameObject.SetActive(!_Data.hiddenData[i]);

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
                    case Const.CurrencyType.PiggyCoin:
                        purchaseText = "GET";
                        break;
                    case Const.CurrencyType.Ticket:
                        purchaseText = "GET";
                        break;
                    case Const.CurrencyType.Local:
                        purchaseText = "BUY";
                        break;
                }
                

                purchaseOption.SetPrice(lookUp.currency.type.IsLocal() ? lookUp.GetLocalPrice() : CurrencyDisplay.GetCurrencyString(lookUp.currency), lookUp.currency.type, available);
                
                // if (glimmerByBadge)
                // {
                    purchaseOption.Glimmer();
                    purchaseOption.SetPurchaseText(purchaseText);
                // }
            }

            maxStackText.text = Board.THIS.StackLimit.ToString();
            capacityText.text = PiggyMenu.THIS._Data.moneyCapacity.ToString();
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
                    PiggyMenu.THIS._Data.moneyCapacity += 5;
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
            _Data.instanceData[index]++;
            AnalyticsManager.PurchasedUpgrade(purchaseType.ToString(), _Data.instanceData[index]);
        }

        public void Prompt(PurchaseType purchaseType, float amount, float delay)
        {
            PurchaseOption purchaseOption = purchaseOptions[(int)purchaseType];
            purchaseOption.PunchColor(Const.THIS.acceptedFrameColor, Const.THIS.defaultFrameColor);
            purchaseOption.PunchScale(amount, delay);
            this.WaitForFrame(() =>
            {
                SnapTo(purchaseOption.animationPivot);
            });
        }
        
        public void SnapTo(RectTransform target)
        {
            // Canvas.ForceUpdateCanvases();
            
            Vector2 dif = (Vector2)scrollRectFrame.transform.InverseTransformPoint(scrollPanel.position) - (Vector2)scrollRectFrame.transform.InverseTransformPoint(target.position);

            scrollRectFrame.anchoredPosition = new Vector2(scrollRectFrame.anchoredPosition.x, dif.y);
        }

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
            // [SerializeField] public ExtraCondition extraCondition;
            [TextArea] [SerializeField] public string title;
            [TextArea] [SerializeField] public string info;
            [TextArea] [SerializeField] public string extra;
            // [SerializeField] public bool best = false;

            public string GetLocalPrice()
            {
                return "";
                // return IAPManager.THIS.GetLocalPrice(purchaseType);
            }
        }

        // public enum ExtraCondition
        // {
        //     ALWAYS,
        //     PIGGY_CAP,
        // }
        
    }
}