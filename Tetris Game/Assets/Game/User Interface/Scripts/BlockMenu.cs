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
        [SerializeField] private Color upgradeColor, purchaseColor;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private CurrenyButton purchaseButton;
        [SerializeField] private RectTransform newTextBanner;
        [SerializeField] private RectTransform equippedTextBanner;
        [SerializeField] private TextMeshProUGUI equipText;
        [System.NonSerialized] private BlockShopData _blockShopData;
        [System.NonSerialized] private BlockData _selectedBlockData;

        public BlockShopData _Data
        {
            set => _blockShopData = value;
            get => this._blockShopData;
        }
        
        public void Set(ref BlockShopData blockShopData)
        {
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

            int showIndex = _blockShopData.lastIndex;
            // string indexStr = showIndex + " / " + Const.THIS.DefaultBlockData.Length;
            _selectedBlockData = Const.THIS.DefaultBlockData[showIndex];
            
            bool availableByLevel = LevelManager.CurrentLevel >= _selectedBlockData.unlockedAt;
            bool availableByPrice = Wallet.HasFunds(_selectedBlockData.Cost);
            bool availableByTicket = _selectedBlockData.Cost.type.Equals(Const.CurrencyType.Ticket);
            bool purchasedBlock = _blockShopData.HaveBlock(_selectedBlockData.blockType);

            bool canPurchase = (availableByPrice || availableByTicket) && availableByLevel;

            
            SetPrice(_selectedBlockData.Cost, canPurchase, availableByTicket);
            SetLookUp(_selectedBlockData.blockType.Prefab<Block>().segmentTransforms);
            
            
            frame.color = purchasedBlock ? upgradeColor : purchaseColor;
            
            newTextBanner.gameObject.SetActive(!purchasedBlock && availableByLevel);

            equippedTextBanner.gameObject.SetActive(!availableByLevel || purchasedBlock);
            equipText.text = purchasedBlock ? Onboarding.THIS.equippedText : Onboarding.THIS.unlockedAtText + _selectedBlockData.unlockedAt;

            if (!purchasedBlock)
            {
                PunchNewBanner(0.4f);
            }

            currencyDisplay.gameObject.SetActive(!purchasedBlock);
            purchaseButton.gameObject.SetActive(!purchasedBlock);

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
        }

        public void OnClick_ShowNext()
        {
            _blockShopData.lastIndex++;
            if (_blockShopData.lastIndex >= Const.THIS.DefaultBlockData.Length)
            {
                _blockShopData.lastIndex = 0;
            }
            Show();
        }
        public void OnClick_ShowPrevious()
        {
            _blockShopData.lastIndex--;
            if (_blockShopData.lastIndex < 0)
            {
                _blockShopData.lastIndex = Const.THIS.DefaultBlockData.Length - 1;
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

        private void SetPrice(Const.Currency currency, bool canPurchase, bool availableByTicket)
        {
            purchaseButton.Available = canPurchase;
            // purchaseButton.ButtonSprite = availableByTicket ? Const.THIS.watchButtonTexture : Const.THIS.getButtonTexture;
            purchaseButton.ButtonSprite = Const.THIS.GetButtonSprite(currency.type);

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
            bool haveBlock = _blockShopData.HaveBlock(_selectedBlockData.blockType);
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
            
            if (Wallet.Consume(_selectedBlockData.Cost))
            {
                _blockShopData.AddUnlockedBlock(_selectedBlockData);
                
                Spawner.THIS.InterchangeBlock(_selectedBlockData.blockType, Pawn.Usage.UnpackedAmmo);

                if (ONBOARDING.PURCHASE_BLOCK.IsNotComplete())
                {
                    ONBOARDING.PURCHASE_BLOCK.SetComplete();
                    Onboarding.HideFinger();
                }
                Show();
                maskFrame.Glimmer(AnimConst.THIS.glimmerSpeedBlock);
                
                AnalyticsManager.PurchasedBlockCount(_blockShopData.UnlockedCount - 5);
            }
            else
            {
                if (_selectedBlockData.Cost.type.Equals(Const.CurrencyType.Ticket))
                {
                    AdManager.ShowTicketAd(() =>
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

            public Const.Currency Cost => currency.ReduceCost(Const.CurrencyType.Coin, Wallet.CostReduction);
        }
    }
}
