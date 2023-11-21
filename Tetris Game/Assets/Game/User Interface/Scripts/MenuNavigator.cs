using System;
using System.Collections.Generic;
using Game.UI;
using Internal.Core;
using UnityEngine;

public class MenuNavigator : Menu<MenuNavigator>, IMenu
{
    [System.NonSerialized] private List<IMenu> _menus = new();
    [SerializeField] private List<Tab> tabs = new();
    [SerializeField] private GameObject costReductionStamp;
    [System.NonSerialized] private Data _data;
    
    [System.NonSerialized] public int TimeScale = 1;

    public bool CostRedStamp
    {
        set => costReductionStamp.SetActive(value);
    }
    
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
        
        TimeScale = 0;
        GameManager.UpdateTimeScale();
        
        Wallet.ScaleTransactors(1.1f, true);
        Activate();
        OpenLastMenu();
    }

    private void Activate()
    {
        tabs[(int)MenuType.Block].gameObject.SetActive(ONBOARDING.BLOCK_TAB.IsComplete());
        tabs[(int)MenuType.Weapon].gameObject.SetActive(ONBOARDING.WEAPON_TAB.IsComplete());
        tabs[(int)MenuType.Upgrade].gameObject.SetActive(ONBOARDING.UPGRADE_TAB.IsComplete());
    }
    
    public new bool Close(float duration = 0.25f, float delay = 0.0f)
    {
        if (base.Close())
        {
            return true;
        }
        UIManager.MenuMode(false);
        
        TimeScale = 1;
        GameManager.UpdateTimeScale();
        
        Wallet.ScaleTransactors(1.0f);
        Onboarding.HideFinger();

        int lastMenuIndex = (int)_Data.lastMenuType;
        tabs[lastMenuIndex].Hide();
        _menus[lastMenuIndex].Close(0.2f);
        
        
        return false;
    }
    
    public void OnClick_Close()
    {
        this.Close();
    }

    public void SetLastMenu(MenuType menuType)
    {
        _Data.lastMenuType = menuType;
    }

    private void OpenLastMenu(float duration = 0.25f)
    {
        int lastMenuIndex = (int)_Data.lastMenuType;
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
        
        Onboarding.HideFinger();

        int index = (int)_data.lastMenuType;
        _menus[index].Close(0.1f, 0.1f);
        tabs[index].Hide();

        _data.lastMenuType = menuTypeNext;
        OpenLastMenu(0.1f);
    }
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
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

