using System;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.UI
{
    public class WeaponMenu : Menu<WeaponMenu>, IMenu
    {
        [Header("Stage Bars")]
        [SerializeField] private Image frame;
        [SerializeField] private StageBar stageBarDamage;
        [SerializeField] private StageBar stageBarFireRate;
        [SerializeField] private StageBar stageBarSplitShot;
        [SerializeField] private Image gunImage;
        [SerializeField] private RectTransform newTextBanner;
        [SerializeField] private RectTransform equippedTextBanner;
        [SerializeField] private RectTransform stageBarParent;
        [SerializeField] private RectTransform purchaseParent;
        [SerializeField] private RectTransform purchaseClickTarget;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private CurrenyButton purchaseButton;
        [SerializeField] private RectTransform priceTextPivot;
        [SerializeField] private RectTransform buttonRectTransform;
        [SerializeField] private Button equipButton;
        [SerializeField] private TextMeshProUGUI gunStatText;

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
        
        public new void Show()
        {
            base.Show();
            CustomShow();
        }

        public void CustomShow(float gunPunchAmount = 0.1f, bool glimmer = false)
        {
            bool purchasedWeapon = _weaponShopData.Purchased;
            bool equippedWeapon = _weaponShopData.Equipped;

            stageBarParent.gameObject.SetActive(purchasedWeapon);
            purchaseParent.gameObject.SetActive(!purchasedWeapon);
            equipButton.gameObject.SetActive(purchasedWeapon && !equippedWeapon);
            
            _gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.gunIndex];
            
            SetSprite(_gunUpgradeData.sprite, gunPunchAmount);
            if (glimmer)
            {
                frame.Glimmer(AnimConst.THIS.glimmerSpeedWeapon);
            }

            newTextBanner.gameObject.SetActive(!purchasedWeapon);
            equippedTextBanner.gameObject.SetActive(equippedWeapon);

            if (!purchasedWeapon)
            {
                PunchNewBanner(0.4f);
            }
            
            int damage = CurrentDamage(_gunUpgradeData);
            int rate = CurrentFireRate(_gunUpgradeData);
            int split = CurrentSplitShot(_gunUpgradeData);

            SetStats(damage, rate, split);
            
            
            if (stageBarParent.gameObject.activeSelf && ONBOARDING.LEARN_TO_PURCHASE_FIRERATE.IsNotComplete())
            {
                if (_gunUpgradeData.HasAvailableUpgrade(Gun.StatType.Firerate, _weaponShopData.CurrentIndex(Gun.StatType.Firerate)))
                {
                    Onboarding.ClickOn(stageBarFireRate.clickTarget.position, false, () =>
                    {
                        stageBarFireRate.PunchPurchaseButton(0.2f);
                    });
                }
                else
                {
                    Onboarding.HideFinger();
                }
            }
            if (purchaseParent.gameObject.activeSelf && ONBOARDING.LEARN_TO_PURCHASE_WEAPON.IsNotComplete())
            {
                if (Wallet.HasFunds(_gunUpgradeData.currency))
                {
                    Onboarding.ClickOn(purchaseClickTarget.position, false, () =>
                    {
                        Transform buttonTransform = purchaseButton.transform;
                        buttonTransform.DOKill();
                        buttonTransform.localEulerAngles = Vector3.zero;
                        buttonTransform.localScale = Vector3.one;
                        buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    });
                }
                else
                {
                    Onboarding.HideFinger();
                }
            }
            
            
            if (!purchasedWeapon)
            {
                SetPrice(_gunUpgradeData.currency);
                return;
            }
            
            FillStageBar(Gun.StatType.Damage, stageBarDamage);
            FillStageBar(Gun.StatType.Firerate, stageBarFireRate);
            FillStageBar(Gun.StatType.Splitshot, stageBarSplitShot);
        }

        private void FillStageBar(Gun.StatType statType, StageBar stageBar)
        {
            int currentIndex = _weaponShopData.CurrentIndex(statType);

            bool max = _gunUpgradeData.IsFull(statType, currentIndex);
            
            stageBar
                .SetMaxed(!max)
                .SetCurrencyStampVisible(!max)
                .SetBars(_gunUpgradeData.UpgradeCount(statType), currentIndex);

            if (max)
            {
                return;
            }
            Const.Currency price = _gunUpgradeData.Price(statType, currentIndex);

            stageBar
                .SetPrice(price)
                .SetInteractable(Wallet.HasFunds(price));
        }
        
        public void SetStats(int damage, int rate, int split)
        {
            StringBuilder stringBuilder = new();
            
            stringBuilder.Append(Onboarding.THIS.damageText);
            stringBuilder.Append(damage);
            
            stringBuilder.Append(Onboarding.THIS.fireRateText);
            stringBuilder.Append(rate);
            
            stringBuilder.Append(Onboarding.THIS.splitShotText);
            stringBuilder.Append(split);


            gunStatText.text = stringBuilder.ToString();
        }
        
        private bool SetPrice(Const.Currency currency)
        {
            bool hasFunds = Wallet.HasFunds(currency);

            purchaseButton.Available = hasFunds;
            
            if (hasFunds)
            {   
                PunchButton(0.2f);
            }
            
            currencyDisplay.Display(currency);
            PunchMoney(0.2f);
            return hasFunds;
        }
        
        private void SetSprite(Sprite sprite, float punchAmount)
        {
            gunImage.sprite = sprite;
            Transform gunImageTransform;
            (gunImageTransform = gunImage.transform).DOKill();
            gunImageTransform.localScale = Vector3.one;
            gunImageTransform.DOPunchScale(Vector3.one * punchAmount, 0.25f, 1).SetUpdate(true);
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
        private void PunchNewBanner(float amount)
        {
            newTextBanner.DOKill();
            newTextBanner.localScale = Vector3.one;
            newTextBanner.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }

        
        public Gun.Data EquippedGunData
        {
            get
            {

                Gun.UpgradeData gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.equipIndex];

                int damage = CurrentDamage(gunUpgradeData);
                int rate = CurrentFireRate(gunUpgradeData);
                int split = CurrentSplitShot(gunUpgradeData);

                Pool gunType = gunUpgradeData.gunType;

                return new Gun.Data(gunType, damage, rate, split);
            }
        }

        private int CurrentDamage(Gun.UpgradeData gunUpgradeData)
        {
            int currentIndex_Damage = _weaponShopData.CurrentIndex(Gun.StatType.Damage);
            int damage = gunUpgradeData.UpgradedValue(Gun.StatType.Damage, currentIndex_Damage);
            return damage;
        }
        
        private int CurrentFireRate(Gun.UpgradeData gunUpgradeData)
        {
            int currentIndex_FireRate = _weaponShopData.CurrentIndex(Gun.StatType.Firerate);
            int rate = gunUpgradeData.UpgradedValue(Gun.StatType.Firerate, currentIndex_FireRate);
            return rate;
        }

        private int CurrentSplitShot(Gun.UpgradeData gunUpgradeData)
        {
            int currentIndex_SplitShot = _weaponShopData.CurrentIndex(Gun.StatType.Splitshot);
            int split = gunUpgradeData.UpgradedValue(Gun.StatType.Splitshot, currentIndex_SplitShot);
            return split;
        }

        public void OnClick_PurchaseUpgrade(int statType)
        {
            Gun.StatType type = (Gun.StatType)statType;

            Const.Currency price = _gunUpgradeData.Price(type, _weaponShopData.CurrentIndex(type));

            if (Wallet.Consume(price))
            {
                _weaponShopData.Upgrade(type, 1);

                if (_weaponShopData.Equipped)
                {
                    OnGunDataChanged?.Invoke(EquippedGunData);
                }
                
                if (ONBOARDING.LEARN_TO_PURCHASE_FIRERATE.IsNotComplete())
                {
                    ONBOARDING.LEARN_TO_PURCHASE_FIRERATE.SetComplete();
                    ONBOARDING.ABLE_TO_USE_UPGRADE_TAB.SetComplete();
                    Onboarding.HideFinger();
                }
                
                CustomShow(0.2f, true);
            }
        }
        
        public void OnClick_PurchaseWeapon()
        {
            // _gunUpgradeData = Const.THIS.GunUpgradeData[_weaponShopData.gunIndex];

            if (_weaponShopData.Purchased)
            {
                return;
            }
            
            if (Wallet.Consume(_gunUpgradeData.currency))
            {
                if (ONBOARDING.LEARN_TO_PURCHASE_WEAPON.IsNotComplete())
                {
                    ONBOARDING.LEARN_TO_PURCHASE_WEAPON.SetComplete();
                    Onboarding.HideFinger();
                }
                
                _weaponShopData.Purchase();

                OnClick_Equip();
            }
        }
        
        public void OnClick_Equip()
        {
            _weaponShopData.Equip();
            OnGunDataChanged?.Invoke(EquippedGunData);
            CustomShow(0.3f, true);
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
            [SerializeField] public int equipIndex;
            [SerializeField] public List<GunShopData> gunShopDatas = new();

            public WeaponShopData()
            {
                
            }
            public WeaponShopData(WeaponShopData weaponShopData)
            {
                gunIndex = weaponShopData.gunIndex;
                equipIndex = weaponShopData.equipIndex;
                gunShopDatas.CopyFrom(weaponShopData.gunShopDatas);
            }
            
            public int CurrentIndex(Gun.StatType statType)
            {
                return gunShopDatas[gunIndex].upgradeIndexes[(int)statType];
            }
            public int GetUpgradeIndexOfEquippedGun(Gun.StatType statType)
            {
                return gunShopDatas[equipIndex].upgradeIndexes[(int)statType];
            }
            public void Upgrade(Gun.StatType statType, int amount)
            {
                gunShopDatas[gunIndex].upgradeIndexes[(int)statType] += amount;
            }
            public void Purchase()
            {
                gunShopDatas[gunIndex].purchased = true;
            }
            public void Equip()
            {
                if (Equipped)
                {
                    return;
                }

                equipIndex = gunIndex;
            }
            public bool Purchased => gunShopDatas[this.gunIndex].purchased;
            public bool Equipped => gunIndex == equipIndex;

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