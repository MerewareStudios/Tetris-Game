using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using Internal.Visuals;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using User;


namespace Game.UI
{
    public class WeaponMenu : Menu<WeaponMenu>
    {
        [Header("Stage Bars")]
        [SerializeField] private StageBar stageBarFireRate;
        [SerializeField] private StageBar stageBarSplitShot;
        [SerializeField] private StageBar stageBarDamage;
        [SerializeField] private Image gunImage;

        [System.NonSerialized] private WeaponShopData _weaponShopData;
        [System.NonSerialized] private Gun.UpgradeData _gunUpgradeData;
        [System.NonSerialized] public System.Action<Gun.Data> OnGunDataChanged = null;

        public void Set(ref WeaponShopData _weaponShopData)
        {
            this._weaponShopData = _weaponShopData;
        }

        public override Menu<WeaponMenu> Open()
        {
            Show();
            return base.Open();
        }

        public void OnClick_Close()
        {
            base.Close();
        }

        private void Show()
        {
            int gunIndex = _weaponShopData.gunIndex;
            
            _gunUpgradeData = Const.THIS.GunUpgradeData[gunIndex];
            
            SetSprite(_gunUpgradeData.sprite);

            FillStageBar(_gunUpgradeData.stageData_Firerate, stageBarFireRate, _weaponShopData.fireRateIndex);
            FillStageBar(_gunUpgradeData.stageData_Splitshot, stageBarSplitShot, _weaponShopData.splitShotIndex);
            FillStageBar(_gunUpgradeData.stageData_Damage, stageBarDamage, _weaponShopData.damageIndex);
        }

        private void FillStageBar<T>(StageBar.StageData<T>[] stageDatas, StageBar stageBar, int currentIndex)
        {
            StageBar.StageData<T> stageData = stageDatas[currentIndex];
            
            bool max = currentIndex >= stageDatas.Length - 1;
            int price = max ? 0 : (stageData.purchaseType.Equals(PurchaseType.Ad) ? -1 : stageData.price);
            
            stageBar
                .SetTopInfo(stageData.value.ToString())
                .SetPurchaseType(stageData.purchaseType)
                .SetPrice(price)
                .SetBars(stageDatas.Length - 1, currentIndex)
                .SetUsable(!max);
        }
        
        public void SetSprite(Sprite sprite)
        {
            gunImage.sprite = sprite;
        }

        private void Upgrade(Gun.StatType statType)
        {
            switch (statType)
            {
                case Gun.StatType.Firerate:
                    _weaponShopData.fireRateIndex++;
                    break;
                case Gun.StatType.Splitshot:
                    _weaponShopData.splitShotIndex++;
                    break;
                case Gun.StatType.Damage:
                    _weaponShopData.damageIndex++;
                    break;
            }

            if (_gunUpgradeData.IsAllFull(_weaponShopData.fireRateIndex, _weaponShopData.splitShotIndex, _weaponShopData.damageIndex))
            {
                _weaponShopData.gunIndex++;
                _weaponShopData.Refresh();
            }

            Gun.Type gunType = _gunUpgradeData.gunType;
            float fireRate = _gunUpgradeData.stageData_Firerate[_weaponShopData.fireRateIndex].value;
            int split = _gunUpgradeData.stageData_Splitshot[_weaponShopData.splitShotIndex].value;
            int damage = _gunUpgradeData.stageData_Damage[_weaponShopData.damageIndex].value;

            Gun.Data gunData = new(gunType, fireRate, split, damage);
            
            OnGunDataChanged?.Invoke(gunData);
        }

        private int GetPrice(Gun.StatType statType)
        {
            switch (statType)
            {
                case Gun.StatType.Firerate:
                    return _gunUpgradeData.stageData_Firerate[_weaponShopData.fireRateIndex].price;
                case Gun.StatType.Splitshot:
                    return _gunUpgradeData.stageData_Splitshot[_weaponShopData.splitShotIndex].price;
                case Gun.StatType.Damage:
                    return _gunUpgradeData.stageData_Damage[_weaponShopData.damageIndex].price;
            }

            Debug.LogError("Stat type is missing");
            return -1;
        }
        
        private void OnPurchase(Gun.StatType statType)
        {
            Upgrade(statType);
            Show();
        }

        public void OnClick_PurchaseWithMoney(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;

            int price = GetPrice(type);
            if (MoneyTransactor.THIS.Transaction(price))
            {
                OnPurchase(type);
            }
        }
        
        public void OnClick_PurchaseWithAd(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;

            Debug.LogWarning("Watch Ad - Not Implemented, price given");
            if (true)
            {
                OnPurchase(type);
            }
        }


        [System.Serializable]
        public class WeaponShopData : ICloneable
        {
            [SerializeField] public int gunIndex;
            [SerializeField] public int fireRateIndex;
            [SerializeField] public int splitShotIndex;
            [SerializeField] public int damageIndex;

            public WeaponShopData()
            {
                
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                
            }

            public void Refresh()
            {
                fireRateIndex = 0;
                splitShotIndex = 0;
                damageIndex = 0;
            }

            public Gun.Data GetCurrentGunData()
            {
                return null;
            }
            
            public object Clone()
            {
                return new WeaponShopData(this);
            }
        } 
        
        
    }
}