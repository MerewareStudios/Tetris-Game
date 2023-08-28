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
        [SerializeField] private RectTransform stageBarParent;
        [SerializeField] private RectTransform purchaseParent;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private CurrenyButton purchaseButton;
        [SerializeField] private RectTransform priceTextPivot;
        [SerializeField] private RectTransform buttonRectTransform;
        
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
            bool purchasedWeapon = _weaponShopData.HaveTheCurrentWeapon;

            purchaseParent.gameObject.SetActive(!purchasedWeapon);
            stageBarParent.gameObject.SetActive(purchasedWeapon);
            
            _gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.gunIndex];
            
            SetSprite(_gunUpgradeData.sprite);

            newText.SetText(purchasedWeapon ? "" : Onboarding.THIS.nextWeaponText);
            ownedText.SetText(purchasedWeapon ? Onboarding.THIS.ownedText : "");
            
            if (!purchasedWeapon)
            {
                SetPrice(_gunUpgradeData.currency);
                return;
            }
            
            FillStageBar(Gun.StatType.Firerate, stageBarFireRate);
            FillStageBar(Gun.StatType.Splitshot, stageBarSplitShot);
            FillStageBar(Gun.StatType.Damage, stageBarDamage);
        }

        private void FillStageBar(Gun.StatType statType, StageBar stageBar)
        {
            int currentIndex = _weaponShopData.GetUpgradeIndex(statType);

            StageBar.StageData<int>[] stageDatas = _gunUpgradeData.stageDatas[(int)statType];
            StageBar.StageData<int> stageData = stageDatas[currentIndex];
            
            bool max = currentIndex >= stageDatas.Length - 1;

            stageBar
                .SetTopInfo(stageData.value.ToString())
                .SetPrice(stageData.currency, max)
                .SetBars(stageDatas.Length - 1, currentIndex);
        }
        
        private bool SetPrice(Const.Currency currency)
        {
            bool hasFunds = Wallet.HasFunds(currency);

            purchaseButton.SetAvailable(hasFunds);
            
            if (hasFunds)
            {   
                PunchButton(0.2f);
            }
            
            currencyDisplay.Display(currency);
            PunchMoney(0.2f);
            return hasFunds;
        }
        
        private void SetSprite(Sprite sprite)
        {
            gunImage.sprite = sprite;
            gunImage.transform.DOKill();
            gunImage.transform.localScale = Vector3.one;
            gunImage.transform.DOPunchScale(Vector3.one * 0.25f, 0.25f, 1).SetUpdate(true);
        }
        
        private void PunchMoney(float amount)
        {
            priceTextPivot.DOKill();
            priceTextPivot.localScale = Vector3.one;
            priceTextPivot.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }
        private void PunchButton(float amount)
        {
            buttonRectTransform.DOKill();
            buttonRectTransform.localEulerAngles = Vector3.zero;
            buttonRectTransform.DOPunchRotation(new Vector3(0.0f, 0.0f, 10.0f), 0.3f, 15).SetUpdate(true);
        }

        private void Upgrade(Gun.StatType statType)
        {
            _weaponShopData.Upgrade(statType, 1);

            // if (_gunUpgradeData.IsAllFull(_weaponShopData.upgradeIndexes))
            // {
            //     _weaponShopData.gunIndex++;
            //     _weaponShopData.Refresh();
            // }

            OnGunDataChanged?.Invoke(GetCurrentGunData());
        }
        
        public void OnClick_PurchaseWeapon()
        {
            _gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.gunIndex];

            if (_weaponShopData.HaveTheCurrentWeapon)
            {
                return;
            }
            
            if (Wallet.Consume(_gunUpgradeData.currency))
            {
                _weaponShopData.Purchase();

                // if (ONBOARDING.LEARN_TO_PURCHASE_BLOCK.IsNotComplete())
                // {
                //     ONBOARDING.LEARN_TO_PURCHASE_BLOCK.SetComplete();
                //     ONBOARDING.USE_BLOCK_TAB.SetComplete();
                //     // MenuNavigator.THIS.SetLastMenu(MenuType.Weapon);
                //     Onboarding.HideFinger();
                // }
                Show();
            }
        }

        private Gun.Data GetCurrentGunData()
        {
            Pool gunType = _gunUpgradeData.gunType;
            int fireRate = _gunUpgradeData.Value(Gun.StatType.Firerate, _weaponShopData.GetUpgradeIndex(Gun.StatType.Firerate));
            int split = _gunUpgradeData.Value(Gun.StatType.Splitshot, _weaponShopData.GetUpgradeIndex(Gun.StatType.Splitshot));
            int damage = _gunUpgradeData.Value(Gun.StatType.Damage, _weaponShopData.GetUpgradeIndex(Gun.StatType.Damage));

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
            StageBar.StageData<int> stageData = _gunUpgradeData.GetStageData(type, _weaponShopData.GetUpgradeIndex(type));

            if (Wallet.Transaction(stageData.currency))
            {
                OnPurchase(type);
            }
        }
        
        public void OnClick_ShowNext()
        {
            _weaponShopData.gunIndex++;
            if (_weaponShopData.gunIndex >= Const.THIS.GunUpgradeData.Length)
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
                _weaponShopData.gunIndex = Const.THIS.GunUpgradeData.Length - 1;
            }
            Show();
        }
        
        [System.Serializable]
        public class WeaponShopData : ICloneable
        {
            [SerializeField] public int gunIndex;
            [SerializeField] public List<GunShopData> gunShopDatas = new();

            public WeaponShopData()
            {
                
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                gunIndex = weaponShopData.gunIndex;
                gunShopDatas.CopyFrom(weaponShopData.gunShopDatas);
            }
            
            public int GetUpgradeIndex(Gun.StatType statType)
            {
                return gunShopDatas[gunIndex].upgradeIndexes[(int)statType];
            }
            public void Upgrade(Gun.StatType statType, int amount)
            {
                gunShopDatas[gunIndex].upgradeIndexes[(int)statType] += amount;
            }
            public void Purchase()
            {
                gunShopDatas[gunIndex].purchased = true;
            }
            public bool HaveTheCurrentWeapon => gunShopDatas[this.gunIndex].purchased;

            public Gun.Data GetCurrentGunData()
            {
                return null;
            }
            
            public object Clone()
            {
                return new WeaponShopData(this);
            }
        } 
        
        [System.Serializable]
        public class GunShopData : ICloneable
        {
            [SerializeField] public bool purchased = false;
            [SerializeField] public int[] upgradeIndexes = new int[3];

            public GunShopData()
            {
                
            }
            public GunShopData(GunShopData gunShopData)
            {
                purchased = gunShopData.purchased;
                upgradeIndexes = gunShopData.upgradeIndexes.Clone() as int[];
            }

            public object Clone()
            {
                return new GunShopData(this);
            }
        } 
    }
}