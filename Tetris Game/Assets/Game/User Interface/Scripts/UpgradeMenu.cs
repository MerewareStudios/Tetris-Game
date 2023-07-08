using System;
using System.Collections.Generic;
using Internal.Core;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;


namespace Game.UI
{
    public class UpgradeMenu : Menu<UpgradeMenu>, IMenu
    {
        [Header("Purchase Options")]
        [SerializeField] private PurchaseOption[] purchaseOptions;
        [System.NonSerialized] private Data _data;

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

        private void Show()
        {
            for (int i = 0; i < purchaseOptions.Length; i++)
            {
                PurchaseOption purchaseOption = purchaseOptions[i];

                PurchaseData purchaseData = _Data.purchaseData[i];
                bool hasFunds = Wallet.HasFunds(purchaseData.purchaseType, purchaseData.price);
                purchaseOption
                    .SetPurchase(purchaseData.purchaseType, purchaseData.price, hasFunds)
                    .SetDetailedInfo(purchaseData.gain);
            }
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            PurchaseOption purchaseOption = purchaseOptions[purchaseIndex];
            PurchaseData purchaseData = _Data.purchaseData[purchaseIndex];
            
            
            
            bool transactionSuccessful = Wallet.Transaction(purchaseData.purchaseType, -purchaseData.price);
            
            purchaseOptions[purchaseIndex].PunchColor(transactionSuccessful ? Const.THIS.acceptedFrameColor : Const.THIS.deniedFrameColor, Const.THIS.defaultFrameColor);
            purchaseOptions[purchaseIndex].Punch(transactionSuccessful ? new Vector3(0.0f, 30.0f) :  new Vector3(-50.0f, 0.0f));

            if (!transactionSuccessful)
            {
                Toast.Show(UIManager.NO_FUNDS_TEXT, 2.25f);
                return;
            }
            
            Toast.Show(purchaseOption.GetPurchaseInfo(purchaseData.gain), 0.5f);
            
            UpgradeType upgradeType = (UpgradeType)purchaseIndex;
            switch (upgradeType)
            {
                case UpgradeType.Heart:
                    Warzone.THIS.GiveHeart(purchaseData.gain);
                    break;
                case UpgradeType.Shield:
                    Warzone.THIS.GiveShield(purchaseData.gain, 15.0f);
                    break;
                case UpgradeType.MaxStack:
                    Board.THIS.MaxStack = purchaseData.gain;
                    break;
                case UpgradeType.SupplyLine:
                    break;
                case UpgradeType.Hole:
                    break;
                case UpgradeType.PiggyLevel:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Show();
        }

        [Serializable]
        public enum UpgradeType
        {
            Heart,
            Shield,
            MaxStack,
            SupplyLine,
            Hole,
            PiggyLevel,
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
            [SerializeField] public UpgradeType upgradeType;
            [SerializeField] public Const.PurchaseType purchaseType;
            [SerializeField] public int price;
            [SerializeField] public int gain;
            [SerializeField] public int purchaseInstance = 0;
            [SerializeField] public int maxPurchase = 0;
            public PurchaseData()
            {
                
            }
            public PurchaseData(PurchaseData purchaseData)
            {
                this.upgradeType = purchaseData.upgradeType;
                this.purchaseType = purchaseData.purchaseType;
                this.price = purchaseData.price;
                this.gain = purchaseData.gain;
                this.purchaseInstance = purchaseData.purchaseInstance;
                this.maxPurchase = purchaseData.maxPurchase;
            }

            public object Clone()
            {
                return new PurchaseData(this);
            }
        } 
        
    }
}