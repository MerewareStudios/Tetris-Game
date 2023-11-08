using System;
using DG.Tweening;
using Internal.Core;
using IWI;
using IWI.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


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
                    .SetBestBadge(lookUp.best)
                    .SetInfo(lookUp.title, lookUp.info);
            }
        }

        private void Show(bool glimmerByBadge = true)
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


                // bool extraCond = true;
                // switch (lookUp.extraCondition)
                // {
                //     case ExtraCondition.ALWAYS:
                //         extraCond = true;
                //         break;
                //     case ExtraCondition.PIGGY_CAP:
                //         // extraCond = PiggyMenu.THIS._Data.moneyCapacity > 10;
                //         extraCond = true;
                //         // string extra = extraCond ? ("\nCurrent\n" + PiggyMenu.THIS._Data.currentMoney.amount + "/" + PiggyMenu.THIS._Data.moneyCapacity) : ("\nNot\nAvailable");
                //         // string extra = ("\n" + PiggyMenu.THIS._Data.currentMoney.amount + "/" + PiggyMenu.THIS._Data.moneyCapacity);
                //         // purchaseOption.SetInfo(lookUp.title, lookUp.info + extra);
                //         break;
                // }
                

                bool available = Wallet.HasFunds(lookUp.currency) || lookUp.currency.type.Equals(Const.CurrencyType.Ticket);
                // available &= extraCond;

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
                
                if (glimmerByBadge)
                {
                    purchaseOption.GlimmerByBadge();
                    purchaseOption.SetPurchaseText(purchaseText);
                }
            }

            maxStackText.text = Board.THIS.StackLimit.ToString();
            capacityText.text = PiggyMenu.THIS._Data.moneyCapacity.ToString();
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[purchaseIndex];
            
            // bool extraCond = true;
            // switch (lookUp.extraCondition)
            // {
            //     case ExtraCondition.ALWAYS:
            //         extraCond = true;
            //         break;
            //     case ExtraCondition.PIGGY_CAP:
            //         extraCond = PiggyMenu.THIS._Data.moneyCapacity > 10;
            //         break;
            // }

            // if (!extraCond)
            // {
            //     purchaseOptions[purchaseIndex].Punch(new Vector3(0.0f, 15.0f));
            //     return;
            // }

            if (lookUp.currency.type.Equals(Const.CurrencyType.Local))
            {
                IAPManager.THIS.Purchase((PurchaseType)purchaseIndex);
                return;
            }
            
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
                case PurchaseType.TICKET_PACK:
                    Wallet.TICKET.Transaction(5);
                    break;
                case PurchaseType.MEDKIT:
                    Warzone.THIS.Player._CurrentHealth += 15;
                    break;
                case PurchaseType.COIN_PACK:
                    Wallet.COIN.Transaction(1500);
                    break;
                case PurchaseType.RESERVED_ONE:
                    break;
                case PurchaseType.RESERVED_TWO:
                    break;
                case PurchaseType.PIGGY_COIN_PACK:
                    Wallet.PIGGY.Transaction(250);
                    break;
                case PurchaseType.PIGGY_CAPACITY_RESET:
                    PiggyMenu.THIS._Data.moneyCapacity += 10;
                    // PiggyMenu.THIS._Data.currentMoney.amount = 0;
                    break;
                case PurchaseType.BASIC_CHEST:
                    Wallet.COIN.Transaction(80);
                    Wallet.PIGGY.Transaction(2);
                    break;
                case PurchaseType.PRIME_CHEST:
                    Wallet.COIN.Transaction(500);
                    Wallet.PIGGY.Transaction(10);
                    Wallet.TICKET.Transaction(5);
                    break;
                case PurchaseType.PRESTIGE_CHEST:
                    Wallet.COIN.Transaction(1250);
                    Wallet.PIGGY.Transaction(30);
                    Wallet.TICKET.Transaction(15);
                    break;
                case PurchaseType.REMOVE_ADS:
                    AdManager.Bypass.Ads();
                    _Data.hiddenData[(int)PurchaseType.REMOVE_ADS] = true;
                    break;
            }

            if (base.Visible)
            {
                Show(false);
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
            TICKET_PACK,
            MEDKIT,
            COIN_PACK,
            RESERVED_ONE,
            RESERVED_TWO,
            PIGGY_COIN_PACK,
            PIGGY_CAPACITY_RESET,
            BASIC_CHEST,
            PRIME_CHEST,
            PRESTIGE_CHEST,
            REMOVE_ADS,
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
            [SerializeField] public bool best = false;

            public string GetLocalPrice()
            {
                return IAPManager.THIS.GetLocalPrice(purchaseType);
            }
        }

        // public enum ExtraCondition
        // {
        //     ALWAYS,
        //     PIGGY_CAP,
        // }
        
    }
}