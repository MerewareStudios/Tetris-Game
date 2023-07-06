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
                purchaseOption.SetPurchase(purchaseData.purchaseType, purchaseData.gainCount);
            }
        }

        public void OnClick_Purchase(int purchaseIndex)
        {
            
        }

        [Serializable]
        public enum UpgradeType
        {
            Heart,
            Shield,
            MaxStack,
            SupplyLine,
            Agility,
            FreeUpgrade,
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
            [SerializeField] public int priceAmount;
            [SerializeField] public int gainCount;
            [SerializeField] public int purchaseCount = 0;
            public PurchaseData()
            {
                
            }
            public PurchaseData(PurchaseData purchaseData)
            {
                this.upgradeType = purchaseData.upgradeType;
                this.purchaseType = purchaseData.purchaseType;
                this.priceAmount = purchaseData.priceAmount;
                this.gainCount = purchaseData.gainCount;
            }

            public object Clone()
            {
                return new PurchaseData(this);
            }
        } 
        
    }
}