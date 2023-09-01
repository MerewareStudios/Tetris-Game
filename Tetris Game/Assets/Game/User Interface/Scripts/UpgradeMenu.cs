using System;
using System.Collections.Generic;
using Internal.Core;
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
                PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[i];

                // bool hasFunds = Wallet.HasFunds(lookUp.currency);

                purchaseOption
                    .SetPurchaseText(lookUp.currency.type.Equals(Const.CurrencyType.Dollar) ? "BUY" : "GET")
                    .SetIcon(lookUp.sprite)
                    .SetBestBadge(lookUp.best)
                    .SetInfo(lookUp.title, lookUp.info);
            }
        }

        public new void Show()
        {
            base.Show();
            for (int i = 0; i < purchaseOptions.Length; i++)
            {
                PurchaseOption purchaseOption = purchaseOptions[i];
                PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[i];

                bool hasFunds = Wallet.HasFunds(lookUp.currency);

                purchaseOption.SetPurchase(lookUp.currency, hasFunds);
            }
            SetOneTimeData();
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            PurchaseData purchaseData = _Data.purchaseData[purchaseIndex];
            PurchaseDataLookUp lookUp = Const.THIS.purchaseDataLookUp[purchaseIndex];


            if (!Wallet.Consume(lookUp.currency))
            {
                purchaseOptions[purchaseIndex].PunchColor(Const.THIS.deniedFrameColor, Const.THIS.defaultFrameColor);
                purchaseOptions[purchaseIndex].Punch(new Vector3(-50.0f, 0.0f));
                return;
            }
            
            purchaseOptions[purchaseIndex].PunchColor(Const.THIS.acceptedFrameColor, Const.THIS.defaultFrameColor);
            purchaseOptions[purchaseIndex].Punch(new Vector3(0.0f, 30.0f));


            switch ((PurchaseType)purchaseIndex)
            {
                case PurchaseType.MaxStack:
                    
                    break;
                case PurchaseType.BuySkipTicket:
                    
                    break;
                case PurchaseType.MedKit:
                    
                    break;
                case PurchaseType.BuyCoin:
                    
                    break;
                case PurchaseType.Shield:
                    
                    break;
                case PurchaseType.BuyPiggyCoin:
                    
                    break;
                case PurchaseType.PiggyCapacity:
                    
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
            // Toast.Show(purchaseOption.GetPurchaseInfo(purchaseData.gain), 0.5f);
            
            // UpgradeType upgradeType = (UpgradeType)purchaseIndex;
            // switch (upgradeType)
            // {
            //     case UpgradeType.Heart:
            //         Warzone.THIS.GiveHeart(purchaseData.gain);
            //         break;
            //     case UpgradeType.Shield:
            //         Warzone.THIS.GiveShield(purchaseData.gain);
            //         break;
            //     case UpgradeType.MaxStack:
            //         Board.THIS.MaxStack = purchaseData.gain;
            //         break;
            //     case UpgradeType.MaxPiggyLevel:
            //         PiggyMenu.THIS.MaxPiggyLevel++;
            //         break;
            //     default:
            //         throw new ArgumentOutOfRangeException();
            // }
            //
            // purchaseData.gain += purchaseData.increasePerUse;
            // purchaseData.purchaseInstance++;
            Show();
        }

        [Serializable]
        public enum PurchaseType
        {
            MaxStack,
            BuySkipTicket,
            MedKit,
            BuyCoin,
            Shield,
            BuyPiggyCoin,
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
            [SerializeField] public int seconds;
        } 
        
    }
}