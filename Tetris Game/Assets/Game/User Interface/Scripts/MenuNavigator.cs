using System;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;

public class MenuNavigator : Menu<MenuNavigator>, IMenu
{
    [System.NonSerialized] private List<IMenu> _menus = new();
    [SerializeField] private List<Tab> tabs = new();
    [System.NonSerialized] private Data _data;

    
    public MenuNavigator Setup()
    {
        _menus.Add(BlockMenu.THIS);
        _menus.Add(WeaponMenu.THIS);
        _menus.Add(UpgradeMenu.THIS);
        return this;
    }

    public new void Open(float duration = 1.0f)
    {
        if (base.Open(0.1f))
        {
            return;
        }

        UIManager.MenuMode(true);
        
        Wallet.ScaleTransactors(1.5f, true);
        Activate();
        OpenLastMenu();
    }

    private void Activate()
    {
        bool blockActive = true;
        bool weaponActive = ONBOARDING.USE_BLOCK_TAB.IsComplete();
        bool upgradeActive = ONBOARDING.USE_UPGRADE_TAB.IsComplete();
        
        tabs[(int)MenuType.Block].gameObject.SetActive(blockActive);
        tabs[(int)MenuType.Weapon].gameObject.SetActive(weaponActive);
        tabs[(int)MenuType.Upgrade].gameObject.SetActive(upgradeActive);
    }
    
    public new bool Close(float duration = 0.25f, float delay = 0.0f)
    {
        if (base.Close())
        {
            return true;
        }
        UIManager.MenuMode(false);
        Wallet.ScaleTransactors(1.0f);
        _menus[(int)_data.lastMenuType].Close(0.2f);
        return false;
    }
    
    public void OnClick_Close()
    {
        this.Close();
    }

    private void OpenLastMenu(float duration = 0.25f)
    {
        int lastMenuIndex = (int)_data.lastMenuType;
        _menus[lastMenuIndex].GetParentContainer().SetAsLastSibling();
        _menus[lastMenuIndex].Open(duration);
        tabs[lastMenuIndex].Show();
    }
    
    public void OnTab_BlockMenu()
    {
        OpenTabMenu(MenuType.Block);
    }
    public void OnTab_WeaponMenu()
    {
        OpenTabMenu(MenuType.Weapon);
    }
    public void OnTab_UpgradeMenu()
    {
        OpenTabMenu(MenuType.Upgrade);
    }

    private void OpenTabMenu(MenuType menuTypeNext)
    {
        if (_data.lastMenuType.Equals(menuTypeNext))
        {
            return;
        }
        int index = (int)_data.lastMenuType;
        _menus[index].Close(0.1f, 0.1f);
        tabs[index].Hide();

        _data.lastMenuType = menuTypeNext;
        OpenLastMenu(0.1f);
    }
    
    public Data _Data
    {
        set
        {
            _data = value;
        }
        get => _data;
    }
    
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public Game.UI.MenuType lastMenuType;

        public Data()
        {
                
        }
        public Data(Data data)
        {
            lastMenuType = data.lastMenuType;
        }   
        public object Clone()
        {
            return new Data(this);
        }
    } 
}

