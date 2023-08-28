using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.UI;
using Internal.Core;
using Internal.Visuals;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using User;


namespace Game.UI
{
    public class WeaponMenu : Menu<WeaponMenu>, IMenu
    {
        [Header("Stage Bars")]
        [SerializeField] private StageBar stageBarFireRate;
        [SerializeField] private StageBar stageBarSplitShot;
        [SerializeField] private StageBar stageBarDamage;
        [SerializeField] private Image gunImage;
        [SerializeField] private TextAnimator_TMP newText;
        [SerializeField] private TextAnimator_TMP ownedText;
        
        [System.NonSerialized] private WeaponShopData _weaponShopData;
        [System.NonSerialized] private Gun.UpgradeData _gunUpgradeData;
        [System.NonSerialized] public System.Action<Gun.Data> OnGunDataChanged = null;

        public void Set(ref WeaponShopData _weaponShopData)
        {
            this._weaponShopData = _weaponShopData;
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
            int gunIndex = _weaponShopData.gunIndex;
            
            _gunUpgradeData = Const.THIS.GunUpgradeData[gunIndex];
            _gunUpgradeData.Init();
            
            SetSprite(_gunUpgradeData.sprite);

            FillStageBar(Gun.StatType.Firerate, stageBarFireRate);
            FillStageBar(Gun.StatType.Splitshot, stageBarSplitShot);
            FillStageBar(Gun.StatType.Damage, stageBarDamage);
        }

        private void FillStageBar(Gun.StatType statType, StageBar stageBar)
        {
            int currentIndex = _weaponShopData.GetIndex(statType);

            StageBar.StageData<int>[] stageDatas = _gunUpgradeData.stageDatas[(int)statType];
            StageBar.StageData<int> stageData = stageDatas[currentIndex];
            
            bool max = currentIndex >= stageDatas.Length - 1;

            stageBar
                .SetTopInfo(stageData.value.ToString())
                .SetPrice(stageData.currency, max)
                .SetBars(stageDatas.Length - 1, currentIndex);
        }
        
        private void SetSprite(Sprite sprite)
        {
            gunImage.sprite = sprite;
        }

        private void Upgrade(Gun.StatType statType)
        {
            _weaponShopData.AddIndex(statType, 1);

            if (_gunUpgradeData.IsAllFull(_weaponShopData.upgradeIndexes))
            {
                _weaponShopData.gunIndex++;
                _weaponShopData.Refresh();
            }

            OnGunDataChanged?.Invoke(GetCurrentGunData());
        }

        private Gun.Data GetCurrentGunData()
        {
            Gun.Type gunType = _gunUpgradeData.gunType;
            int fireRate = _gunUpgradeData.Value(Gun.StatType.Firerate, _weaponShopData.GetIndex(Gun.StatType.Firerate));
            int split = _gunUpgradeData.Value(Gun.StatType.Splitshot, _weaponShopData.GetIndex(Gun.StatType.Splitshot));
            int damage = _gunUpgradeData.Value(Gun.StatType.Damage, _weaponShopData.GetIndex(Gun.StatType.Damage));

            return new Gun.Data(gunType, fireRate, split, damage);
        }

        private void OnPurchase(Gun.StatType statType)
        {
            Upgrade(statType);
            Show();
            
            // UIManager.THIS.shopBar.Consume();
        }

        public void OnClick_Purchase(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;
            StageBar.StageData<int> stageData = _gunUpgradeData.GetStageData(type, _weaponShopData.GetIndex(type));

            if (Wallet.Transaction(stageData.currency))
            {
                OnPurchase(type);
            }
        }
        
        public void OnClick_ShowNext()
        {
            _weaponShopData.gunIndex++;
            if (_weaponShopData.gunIndex >= Const.THIS.DefaultBlockData.Length)
            {
                _weaponShopData.gunIndex = 0;
            }
            Show();
        }
        public void OnClick_ShowPrevious()
        {
            _weaponShopData.gunIndex--;
            if (_weaponShopData.gunIndex < 0)
            {
                _weaponShopData.gunIndex = Const.THIS.DefaultBlockData.Length - 1;
            }
            Show();
        }
        
        [System.Serializable]
        public class WeaponShopData : ICloneable
        {
            [SerializeField] public int gunIndex;
            [SerializeField] public int[] upgradeIndexes = new int[3];

            public WeaponShopData()
            {
                
            }
            public int GetIndex(Gun.StatType statType)
            {
                return upgradeIndexes[(int)statType];
            }
            public void AddIndex(Gun.StatType statType, int amount)
            {
                upgradeIndexes[(int)statType] += amount;
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                upgradeIndexes = weaponShopData.upgradeIndexes.Clone() as int[];
            }

            public void Refresh()
            {
                for (int i = 0; i < upgradeIndexes.Length; i++)
                {
                    upgradeIndexes[i] = 0;
                }
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