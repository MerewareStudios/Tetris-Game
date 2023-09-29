using System;
using System.Collections.Generic;
using Internal.Core;
using IWI;
using UnityEngine;


namespace Game.UI
{
    public class UpgradeMenu : Menu<UpgradeMenu>, IMenu
    {
        [Header("Purchase Options")]
        [SerializeField] private PurchaseOption[] purchaseOptions;
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
                    case Const.CurrencyType.Dollar:
                        purchaseText = "BUY";
                        break;
                }

                purchaseOption.SetPurchase(lookUp.currency, hasFunds);
                if (glimmerByBadge)
                {
                    purchaseOption.GlimmerByBadge();
                    purchaseOption.SetPurchaseText(purchaseText);

                }
            }
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            // PurchaseData purchaseData = _Data.purchaseData[purchaseIndex];
            PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[purchaseIndex];


            Debug.Log("c");
            if (!Wallet.Consume(lookUp.currency))
            {
                Debug.Log("b");

                if (lookUp.currency.type.Equals(Const.CurrencyType.Ticket))
                {
                    Debug.Log("a");
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


            switch ((PurchaseType)purchaseIndex)
            {
                case PurchaseType.MaxStack:
                    Board.THIS._Data.maxStack++;
                    break;
                case PurchaseType.SkipTicket:
                    
                    break;
                case PurchaseType.MedKit:
                    
                    break;
                case PurchaseType.Coin:
                    
                    break;
                case PurchaseType.Shield:
                    
                    break;
                case PurchaseType.PiggyCoin:
                    
                    break;
                case PurchaseType.PiggyCapacity:
                    Debug.Log("cap");

                    PiggyMenu.THIS._Data.moneyCapacity -= 10;
                    PiggyMenu.THIS._Data.moneyCapacity = Mathf.Clamp(PiggyMenu.THIS._Data.moneyCapacity, 0, 10);
                    PiggyMenu.THIS._Data.currentMoney.amount = Mathf.Min(PiggyMenu.THIS._Data.currentMoney.amount, PiggyMenu.THIS._Data.moneyCapacity);
                    break;
                case PurchaseType.BasicChest:
                    
                    break;
                case PurchaseType.PrimeChest:
                    
                    break;
                case PurchaseType.PrestigeChest:
                    
                    break;
                case PurchaseType.RemoveAds:
                    
                    break;
            }
            Show(false);
        }

        [Serializable]
        public enum PurchaseType
        {
            MaxStack,
            SkipTicket,
            MedKit,
            Coin,
            Heart,
            Shield,
            PiggyCoin,
            PiggyCapacity,
            BasicChest,
            PrimeChest,
            PrestigeChest,
            RemoveAds,
        }

        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] public List<PurchaseData> purchaseData = new();
            public Data()
            {
                
            }
            public Data(Data data)
            {
                purchaseData.CopyFrom(data.purchaseData);   
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
        [System.Serializable]
        public class PurchaseData : ICloneable
        {
            [SerializeField] public long purchaseStamp;
            
            public PurchaseData()
            {
                
            }
            public PurchaseData(PurchaseData purchaseData)
            {
                this.purchaseStamp = purchaseData.purchaseStamp;
            }

            public object Clone()
            {
                return new PurchaseData(this);
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
            [SerializeField] public bool best = false;
        } 
        
    }
}