using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using Internal.Visuals;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using User;


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