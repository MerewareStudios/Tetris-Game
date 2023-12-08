using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using IWI;
using IWI.Tutorial;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.UI
{
    public class BlockMenu : Menu<BlockMenu>, IMenu
    {
        [SerializeField] private Image maskFrame;
        [SerializeField] private BlockVisualGrid blockVisualGrid;
        [SerializeField] private RectTransform priceTextPivot;
        [SerializeField] private RectTransform buttonRectTransform;
        [SerializeField] private RectTransform buttonClickTarget;
        [SerializeField] private Image frame;
        [SerializeField] private Color upgradeColor, purchaseColor, lockedColor;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private CurrenyButton purchaseButton;
        [SerializeField] private RectTransform newTextBanner;
        [SerializeField] private RectTransform equippedTextBanner;
        [SerializeField] private TextMeshProUGUI equipText;
        [SerializeField] private RectTransform redDot;
        [System.NonSerialized] private BlockData _selectedBlockData;
        [System.NonSerialized] private int _lastBlockIndexShown = -1;

        [field: System.NonSerialized] public BlockShopData SavedData { set; get; }

        public int AvailablePurchaseCount(bool updatePage)
        {
            base.TotalNotify = 0;
            for (int i = 0; i < Const.THIS.DefaultBlockData.Length; i++)
            {
                BlockData lookUp = Const.THIS.DefaultBlockData[i];
                Const.Currency cost = lookUp.Cost;

                bool purchased = SavedData.unlockedBlocks.Contains(lookUp.blockType);
                bool hasFunds = Wallet.HasFunds(cost);
                bool ticketType = lookUp.CostType.Equals(Const.CurrencyType.Ticket);
                bool availableByLevel = LevelManager.CurrentLevel >= lookUp.unlockedAt;

                if (!purchased && (hasFunds || ticketType) && availableByLevel)
                {
                    if (updatePage && _lastBlockIndexShown < i)
                    {
                        SavedData.lastIndex = i;
                        _lastBlockIndexShown = i;
                    }

                    base.TotalNotify++;
                }

                if (!availableByLevel)
                {
                    break;
                }
            }

            
            return base.TotalNotify;
        }
        
        public bool Marked
        {
            set
            {
                redDot.DOKill();
                redDot.localScale = Vector3.zero;
                if (value)
                {
                    redDot.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
                }
            }
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

        public new void Show()
        {
            base.Show();

            _selectedBlockData = Const.THIS.DefaultBlockData[SavedData.lastIndex];
            Const.Currency cost = _selectedBlockData.Cost;
            
            bool availableByLevel = LevelManager.CurrentLevel >= _selectedBlockData.unlockedAt;
            bool availableByPrice = Wallet.HasFunds(cost);
            bool availableByTicket = cost.type.Equals(Const.CurrencyType.Ticket);
            bool purchasedBlock = SavedData.HaveBlock(_selectedBlockData.blockType);

            bool canPurchase = (availableByPrice || availableByTicket) && availableByLevel;


            SetPrice(cost, canPurchase, availableByLevel, purchasedBlock);
            SetLookUp(_selectedBlockData.blockType.Prefab<Block>().segmentTransforms);
            
            
            frame.color = availableByLevel ? (purchasedBlock ? upgradeColor : purchaseColor) : lockedColor;

            bool newBannerVisible = !purchasedBlock && availableByLevel;
            newTextBanner.gameObject.SetActive(newBannerVisible);
            
            Marked = canPurchase;

            equippedTextBanner.gameObject.SetActive(!availableByLevel || purchasedBlock);
            equipText.text = purchasedBlock ? Onboarding.THIS.hasText : Onboarding.THIS.unlockedAtText +  _selectedBlockData.unlockedAt;

            if (!purchasedBlock)
            {
                PunchNewBanner(0.4f);
            }

            if (ONBOARDING.PURCHASE_BLOCK.IsNotComplete())
            {
                if (!purchasedBlock && canPurchase)
                {
                    Onboarding.ClickOn(buttonClickTarget.position, Finger.Cam.UI, () =>
                    {
                        purchaseButton.transform.DOKill();
                        purchaseButton.transform.localEulerAngles = Vector3.zero;
                        purchaseButton.transform.localScale = Vector3.one;
                        purchaseButton.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    });
                }
                else
                {
                    Onboarding.HideFinger();
                }
            }
            
            UIManager.UpdateNotifications();
        }

        public void OnClick_ShowNext()
        {
            SavedData.lastIndex++;
            if (SavedData.lastIndex >= Const.THIS.DefaultBlockData.Length)
            {
                SavedData.lastIndex = 0;
            }
            Show();
        }
        public void OnClick_ShowPrevious()
        {
            SavedData.lastIndex--;
            if (SavedData.lastIndex < 0)
            {
                SavedData.lastIndex = Const.THIS.DefaultBlockData.Length - 1;
            }
            Show();
        }
        private void PunchMoney(float amount)
        {
            priceTextPivot.DOKill();
            priceTextPivot.localScale = Vector3.one;
            priceTextPivot.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }
        private void PunchPurchasedText(float amount)
        {
            equippedTextBanner.DOKill();
            equippedTextBanner.localScale = Vector3.one;
            equippedTextBanner.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
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

        private void SetPrice(Const.Currency currency, bool canPurchase, bool availableByLevel, bool purchasedBlock)
        {
            purchaseButton.gameObject.SetActive(availableByLevel && !purchasedBlock);
            currencyDisplay.gameObject.SetActive(availableByLevel && !purchasedBlock);
            if (!availableByLevel || purchasedBlock)
            {
                return;
            }
            purchaseButton.Available = canPurchase;

            if (canPurchase)
            {   
                PunchButton(0.2f);
            }
            currencyDisplay.Display(currency);
            PunchMoney(0.2f);
            PunchPurchasedText(0.2f);
        }
        private void SetLookUp(List<Transform> segments)
        {
            blockVisualGrid.Display(segments);
        }

        public void OnClick_Purchase()
        {
            bool haveBlock = SavedData.HaveBlock(_selectedBlockData.blockType);
            if (haveBlock)
            {
                return;
            }
            bool availableByLevel = LevelManager.CurrentLevel >= _selectedBlockData.unlockedAt;
            if (!availableByLevel)
            {
                PunchPurchasedText(0.25f);
                return;
            }

            Const.Currency cost = _selectedBlockData.Cost;
            if (Wallet.Consume(cost))
            {
                SavedData.AddUnlockedBlock(_selectedBlockData);
                
                Spawner.THIS.InterchangeBlock(_selectedBlockData.blockType, Pawn.Usage.UnpackedAmmo);

                if (ONBOARDING.PURCHASE_BLOCK.IsNotComplete())
                {
                    ONBOARDING.PURCHASE_BLOCK.SetComplete();
                    Onboarding.HideFinger();
                }
                Show();

                maskFrame.Glimmer(AnimConst.THIS.glimmerSpeedBlock);
                
                AnalyticsManager.PurchasedBlockCount(SavedData.UnlockedCount - 5);
            }
            else
            {
                if (cost.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(AdBreakScreen.AdReason.BLOCK_BUY,() =>
                    {
                        Wallet.Transaction(Const.Currency.OneAd);
                        OnClick_Purchase();
                    });
                }
            }
        }

        [System.Serializable]
        public class BlockShopData : ICloneable
        {
            [SerializeField] public List<Pool> unlockedBlocks = new();
            [SerializeField] public int lastIndex = 0;

            public int UnlockedCount => unlockedBlocks.Count;
            
            public BlockShopData()
            {
                
            }
            public BlockShopData(BlockShopData blockShopData)
            {
                unlockedBlocks = new List<Pool>(blockShopData.unlockedBlocks);
                lastIndex = blockShopData.lastIndex;
            }
            public Pool GetRandomBlock()
            {
                return unlockedBlocks.Random<Pool>();
            }
            public bool AddUnlockedBlock(BlockData blockData)
            {
                if (HaveBlock(blockData.blockType))
                {
                    return false;
                }
                unlockedBlocks.Add(blockData.blockType);
                return true;
            }
            public bool HaveBlock(Pool pool)
            {
                return unlockedBlocks.Contains(pool);
            }
            public object Clone()
            {
                return new BlockShopData(this);
            }
        } 
        
        [Serializable]
        public class BlockData
        {
            [SerializeField] public Pool blockType;
            [SerializeField] private Const.Currency currency;
            [SerializeField] public int unlockedAt = 1;

            public Const.Currency Cost => currency;
            public Const.CurrencyType CostType => currency.type;
        }
    }
}
