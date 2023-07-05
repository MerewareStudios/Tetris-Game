using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using Internal.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using User;


namespace Game.UI
{
    public class BlockMenu : Menu<BlockMenu>, IMenu
    {
        [SerializeField] private BlockVisualGrid blockVisualGrid;
        [SerializeField] private RectTransform priceTextPivot;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private Image frame;
        [SerializeField] private Color upgradeColor, purchaseColor;
        [SerializeField] private TextMeshProUGUI upgradeText;
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
            
            SetPrice(_blockData.basePrice);
            SetLookUp(_blockData.lookUp);

            frame.color = _blockData.purchased ? upgradeColor : purchaseColor;
            upgradeText.gameObject.SetActive(_blockData.purchased);
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

        private void SetPrice(int amount)
        {
            priceText.text = amount.CoinAmount();
            PunchMoney(0.15f);
        }
        private void SetLookUp(int[] table)
        {
            blockVisualGrid.Display(table);
        }

        private void OnPurchase()
        {
            _purchaseAction?.Invoke();
            _ = _blockData.purchased ? _blockData.Upgrade() : _blockShopData.AddUnlockedBlock(_blockData);
            Show();
            
            UIManager.THIS.shopBar.ConsumeFill();
        }

        public void PurchaseWithMoney()
        {
            if (UIManager.THIS.currenyTransactorCoin.Transaction(-_blockData.basePrice))
            {
                OnPurchase();
            }
        }
        public void PurchaseWithAd()
        {
            Debug.LogWarning("Watch Ad - Not Implemented, price given");
            if (true)
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
            [SerializeField] public int maxMerge = 6;

            public BlockShopData()
            {
                
            }
            public BlockShopData(BlockShopData blockShopData)
            {
                blockDatas.CopyFrom(blockShopData.blockDatas);
                unlockedBlocks = new List<Pool>(blockShopData.unlockedBlocks);
                lastIndex = blockShopData.lastIndex;
                maxMerge = blockShopData.maxMerge;
            }
            
            public Pool GetRandomBlock()
            {
                return unlockedBlocks.Random<Pool>();
            }

            public bool AddUnlockedBlock(BlockData blockData)
            {
                if (!unlockedBlocks.Contains(blockData.blockType))
                {
                    unlockedBlocks.Add(blockData.blockType);
                    blockData.purchased = true;
                }

                return true;
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
            [SerializeField] public bool purchased = false;
            [SerializeField] public int basePrice;
            [SerializeField] public int[] lookUp;
            [SerializeField] public int upgradeIndex = -1;
            
            public BlockData()
            {
                
            }
            public BlockData(BlockData blockData)
            {
                this.purchased = blockData.purchased;
                this.blockType = blockData.blockType;
                this.basePrice = blockData.basePrice;
                this.lookUp = blockData.lookUp.Clone() as int[];
            }
            public bool Upgrade()
            {
                for (int i = 0; i < lookUp.Length; i++)
                {
                    if (lookUp[i] > 0 && i > upgradeIndex)
                    {
                        lookUp[i]++;
                        upgradeIndex = i;
                        if (upgradeIndex >= lookUp.Length - 1)
                        {
                            upgradeIndex = -1;
                        }
                        return true;
                    }
                }
                upgradeIndex = -1;

                return false;
            }
            public object Clone()
            {
                return new BlockData(this);
            }
        }
    }
}
