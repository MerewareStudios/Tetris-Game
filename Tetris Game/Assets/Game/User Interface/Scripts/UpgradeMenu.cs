using System;
using JetBrains.Annotations;


namespace Game.UI
{
    public class UpgradeMenu : Menu<UpgradeMenu>, IMenu
    {
        // [Header("Stage Bars")]
        [System.NonSerialized] [CanBeNull] private UpgradeShopData _upgradeShopData;

        public void Set(ref UpgradeShopData upgradeShopData)
        {
            this._upgradeShopData = upgradeShopData;
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
            
        }

        [Serializable]
        public enum UpgradeType
        {
            Heart,
            Shield,
            MaxStack,
            SupplyLine,
            Agility,
            Luck,
            FreeUpgrade,
        }

        [System.Serializable]
        public class UpgradeShopData : ICloneable
        {

            public UpgradeShopData()
            {
                
            }
            public UpgradeShopData(UpgradeShopData upgradeShopData)
            {
                
            }

            public object Clone()
            {
                return new UpgradeShopData(this);
            }
        } 
        
        
    }
}