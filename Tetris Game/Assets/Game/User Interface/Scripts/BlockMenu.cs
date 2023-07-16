using System;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Game.UI
{
    public class BlockMenu : Menu<BlockMenu>, IMenu
    {
        [SerializeField] private BlockVisualGrid blockVisualGrid;
        [SerializeField] private RectTransform priceTextPivot;
        [SerializeField] private CurrencyDisplay currencyDisplay;
        [SerializeField] private Image frame;
        [SerializeField] private Color upgradeColor, purchaseColor;
        [SerializeField] private TextMeshProUGUI upgradeText;
        [SerializeField] private CurrenyButton purchaseButton;
        [System.NonSerialized] private BlockShopData _blockShopData;
        [System.NonSerialized] private System.Action _purchaseAction = null;
        [System.NonSerialized] private BlockData _blockData;

        public void Set(ref BlockShopData blockShopData)
        {
            this._blockShopData = blockShopData;
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
            int showIndex = _blockShopData.lastIndex;
            this._blockData = _blockShopData.blockDatas[showIndex];
            
            SetPrice(_blockData.currency);
            SetLookUp(_blockData.lookUp);

            bool purchasedBlock = _blockShopData.HaveBlock(_blockData.blockType);
            
            frame.color = purchasedBlock ? upgradeColor : purchaseColor;
            upgradeText.gameObject.SetActive(purchasedBlock);
        }

        public void OnClick_ShowNext()
        {
            _blockShopData.lastIndex++;
            if (_blockShopData.lastIndex >= _blockShopData.blockDatas.Count)
            {
                _blockShopData.lastIndex = 0;
            }
            Show();
        }
        
        private void PunchMoney(float amount)
        {
            priceTextPivot.DOKill();
            priceTextPivot.localScale = Vector3.one;
            priceTextPivot.DOPunchScale(Vector3.one * amount, 0.25f, 1).SetUpdate(true);
        }
        
        public void OnClick_ShowPrevious()
        {
            _blockShopData.lastIndex--;
            if (_blockShopData.lastIndex < 0)
            {
                _blockShopData.lastIndex = _blockShopData.blockDatas.Count - 1;
            }
            Show();
        }

        private void SetPrice(Const.Currency currency)
        {
            bool hasFunds = Wallet.HasFunds(currency);

            purchaseButton.SetAvailable(hasFunds);
            
            currencyDisplay.Display(currency);
            PunchMoney(0.15f);
        }
        private void SetLookUp(int[] table)
        {
            blockVisualGrid.Display(table);
        }

        private void OnPurchase()
        {
            _purchaseAction?.Invoke();

            bool purchasedBefore =  _blockShopData.HaveBlock(_blockData.blockType);
            
            _ = purchasedBefore ? _blockData.Upgrade() : _blockShopData.AddUnlockedBlock(_blockData);
            
            Toast.Show(purchasedBefore ? "BLOCK UPGRADED" : "BLOCK ADDED", 0.5f);
            
            Show();
            
            UIManager.THIS.shopBar.ConsumeFill();
        }

        public void OnClick_Purchase()
        {
            if (Wallet.Transaction(_blockData.currency))
            {
                OnPurchase();
            }
        }
        
        
        [System.Serializable]
        public class BlockShopData : ICloneable
        {
            [SerializeField] public List<BlockData> blockDatas = new();
            [SerializeField] public List<Pool> unlockedBlocks = new();
            [SerializeField] public int lastIndex = 0;
            [System.NonSerialized] public Dictionary<Pool, BlockData> blockDataDic;

            public BlockShopData()
            {
                
            }
            public BlockShopData(BlockShopData blockShopData)
            {
                blockDatas.CopyFrom(blockShopData.blockDatas);
                unlockedBlocks = new List<Pool>(blockShopData.unlockedBlocks);
                lastIndex = blockShopData.lastIndex;
            }

            private void FillDic()
            {
                blockDataDic = new Dictionary<Pool, BlockData>();
                foreach (var blockData in blockDatas)
                {
                    blockDataDic.Add(blockData.blockType, blockData);
                }
            }
            public Pool GetRandomBlock()
            {
                return unlockedBlocks.Random<Pool>();
            }
            public int[] LookUps(Pool pool)
            {
                if (blockDataDic == null)
                {
                    FillDic();
                }
                return blockDataDic[pool].lookUp;
            }
            public bool AddUnlockedBlock(BlockData blockData)
            {
                if (HaveBlock(blockData.blockType)) return false;
                
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
        public class BlockData : ICloneable
        {
            [SerializeField] public Pool blockType;
            [SerializeField] public Const.Currency currency;
            [SerializeField] public int[] lookUp;
            [SerializeField] public int trackIndex = 0;
            
            public BlockData()
            {
                
            }
            public BlockData(BlockData blockData)
            {
                this.blockType = blockData.blockType;
                this.currency = blockData.currency;
                this.lookUp = blockData.lookUp.Clone() as int[];
                this.trackIndex = blockData.trackIndex;
            }
            public bool Upgrade()
            {
                if (trackIndex >= lookUp.Length)
                {
                    trackIndex = 0;
                }
                if (lookUp[trackIndex] <= 0)
                {
                    trackIndex++;
                    Upgrade();
                    return false;
                }
                lookUp[trackIndex]++;
                trackIndex++;
                return true;
            }
            public object Clone()
            {
                return new BlockData(this);
            }
        }
    }
}
