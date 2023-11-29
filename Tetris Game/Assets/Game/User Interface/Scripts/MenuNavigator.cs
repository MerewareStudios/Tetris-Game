using System.Collections.Generic;
using Game.UI;
using Internal.Core;
using IWI;
using UnityEngine;

public class MenuNavigator : Menu<MenuNavigator>, IMenu
{
    [System.NonSerialized] private List<IMenu> _menus = new();
    [SerializeField] private List<Tab> tabs = new();
    [System.NonSerialized] private Data _savedData;
    [System.NonSerialized] public int TimeScale = 1;
    [Header("Game Notifications")]
    [SerializeField] private GameNotification gameNotificationShop;
    [SerializeField] private GameNotification[] gameNotifications;
    [Header("Offer")]
    [SerializeField] private LockedMiniOffer lockedMiniOffer;

    public Data SavedData
    {
        set
        {
            _savedData = value;
            lockedMiniOffer.SavedData = _savedData.lockedMiniOffer;
            lockedMiniOffer.GetCurrent = () => LevelManager.CurrentLevel;
            lockedMiniOffer.GetAdState = () => AdManager.THIS._Data.removeAds;
        }
        get => _savedData;
    }
    
    public MenuNavigator Setup()
    {
        _menus.Add(BlockMenu.THIS);
        _menus.Add(WeaponMenu.THIS);
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
        
        lockedMiniOffer.Set();
    }

    public void UpdateNotifyMenus(bool visible)
    {
        int block = _menus[0].AvailablePurchaseCount(!visible);
        int weapon = _menus[1].AvailablePurchaseCount(!visible);
        if (visible)
        {
            gameNotifications[0].Count = block;
            gameNotifications[1].Count = weapon;
        }
        else
        {
            gameNotifications[0].CountImmediate = block;
            gameNotifications[1].CountImmediate = weapon;
        }
    }

    public void UpdateTotalNotify()
    {
        int total = 0;
        foreach (var menu in _menus)
        {
            total += menu.TotalNotify;
        }
        gameNotificationShop.Count = total;
    }
    
    private void Activate()
    {
        tabs[(int)MenuType.Block].gameObject.SetActive(ONBOARDING.BLOCK_TAB.IsComplete());
        tabs[(int)MenuType.Weapon].gameObject.SetActive(ONBOARDING.WEAPON_TAB.IsComplete());
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

        int lastMenuIndex = (int)SavedData.lastMenuType;
        tabs[lastMenuIndex].Hide();
        _menus[lastMenuIndex].Close(0.2f);

        
        lockedMiniOffer.Pause();
        
        return false;
    }
    
    public void OnClick_Close()
    {
        this.Close();
    }

    public void SetLastMenu(MenuType menuType)
    {
        SavedData.lastMenuType = menuType;
    }

    private void OpenLastMenu(float duration = 0.25f)
    {
        int lastMenuIndex = (int)SavedData.lastMenuType;
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

    private void OpenTabMenu(MenuType menuTypeNext)
    {
        if (_savedData.lastMenuType.Equals(menuTypeNext))
        {
            return;
        }
        
        Onboarding.HideFinger();

        int index = (int)_savedData.lastMenuType;
        _menus[index].Close(0.1f, 0.1f);
        tabs[index].Hide();

        _savedData.lastMenuType = menuTypeNext;
        OpenLastMenu(0.1f);
    }
    void Update()
    {
        Shader.SetGlobalFloat(Helper.UnscaledTime, Time.unscaledTime);
    }
   
    
    
    [System.Serializable]
    public class Data : System.ICloneable
    {
        [SerializeField] public Game.UI.MenuType lastMenuType;
        [SerializeField] public LockedMiniOffer.Data lockedMiniOffer;

        public Data()
        {
                
        }
        public Data(Data data)
        {
            lastMenuType = data.lastMenuType;
            lockedMiniOffer = data.lockedMiniOffer.Clone() as LockedMiniOffer.Data;
        }   
        public object Clone()
        {
            return new Data(this);
        }
    } 
}

