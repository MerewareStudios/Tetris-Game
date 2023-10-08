using System;
using System.Collections.Generic;
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
        [SerializeField] private TextMeshProUGUI maxStackText;
        [System.NonSerialized] private Data _data;
        [System.NonSerialized] private bool oneTimeDataSet = false;

        public Data _Data
        {
            set
            {
                _data = value;
            }
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

                bool hasFunds = Wallet.HasFunds(lookUp.currency) || lookUp.currency.type.Equals(Const.CurrencyType.Ticket);
                
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

                UseCondition useCondition = lookUp.useCondition;
                switch (useCondition)
                {
                    case UseCondition.Always:
                        
                        break;
                    case UseCondition.HasPiggyCapacity:
                        hasFunds &= PiggyMenu.THIS._Data.moneyCapacity > 10;
                        break;
                }

                if (lookUp.currency.type.Equals(Const.CurrencyType.Local))
                {
                    purchaseOption.SetLocalPrice(lookUp.GetLocalPrice(), hasFunds);
                }
                else
                {
                    purchaseOption.SetPrice(lookUp.currency, hasFunds);
                }
                
                if (glimmerByBadge)
                {
                    purchaseOption.GlimmerByBadge();
                    purchaseOption.SetPurchaseText(purchaseText);
                }
            }

            maxStackText.text = Board.THIS._Data.maxStack.ToString();
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            // PurchaseData purchaseData = _Data.purchaseData[purchaseIndex];
            PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[purchaseIndex];

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
            Debug.Log("Purchased : " + purchaseType);
            switch (purchaseType)
            {
                case PurchaseType.MaxStack:
                    Board.THIS._Data.maxStack++;
                    break;
                case PurchaseType.TicketPack:
                    Wallet.TICKET.Transaction(15);
                    break;
                case PurchaseType.MedKit:
                    Warzone.THIS.Player._CurrentHealth += 15;
                    break;
                case PurchaseType.CoinPack:
                    Wallet.COIN.Transaction(1500);
                    break;
                case PurchaseType.Reserved1:
                    break;
                case PurchaseType.Reserved2:
                    break;
                case PurchaseType.PiggyCoinPack:
                    Wallet.PIGGY.Transaction(250);
                    break;
                case PurchaseType.PiggyCapacity:
                    PiggyMenu.THIS._Data.moneyCapacity -= 50;
                    PiggyMenu.THIS._Data.moneyCapacity = Mathf.Clamp(PiggyMenu.THIS._Data.moneyCapacity, 0, 10);
                    PiggyMenu.THIS._Data.currentMoney.amount = Mathf.Min(PiggyMenu.THIS._Data.currentMoney.amount, PiggyMenu.THIS._Data.moneyCapacity);
                    break;
                case PurchaseType.BasicChest:
                    Wallet.COIN.Transaction(100);
                    Wallet.PIGGY.Transaction(5);
                    break;
                case PurchaseType.PrimeChest:
                    Wallet.COIN.Transaction(750);
                    Wallet.PIGGY.Transaction(150);
                    Wallet.TICKET.Transaction(10);
                    break;
                case PurchaseType.PrestigeChest:
                    Wallet.COIN.Transaction(1000);
                    Wallet.PIGGY.Transaction(250);
                    Wallet.TICKET.Transaction(25);
                    break;
                case PurchaseType.RemoveAdBreak:
                    AdManager.Bypass.Ads();
                    _Data.hiddenData[(int)PurchaseType.RemoveAdBreak] = true;
                    break;
                default:
                    break;
            }

            if (base.Visible)
            {
                Show(false);
            }
        }

        [Serializable]
        public enum PurchaseType
        {
            MaxStack,
            TicketPack,
            MedKit,
            CoinPack,
            Reserved1,
            Reserved2,
            PiggyCoinPack,
            PiggyCapacity,
            BasicChest,
            PrimeChest,
            PrestigeChest,
            RemoveAdBreak,
        }

        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public bool[] hiddenData;
            
            public Data(Data data)
            {
                hiddenData = data.hiddenData.Clone() as bool[];
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
            [SerializeField] public UseCondition useCondition;
            [SerializeField] public bool best = false;

            public string GetLocalPrice()
            {
                return IAPManager.THIS.GetLocalPrice(purchaseType);
            }
        } 
        
    }

    public enum UseCondition
    {
        Always,
        HasPiggyCapacity
    }
}