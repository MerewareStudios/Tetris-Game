using System;
using System.Collections;
using System.Collections.Generic;
using Game.UI;
using Internal.Core;
using UnityEngine;

public class MenuNavigator : Menu<MenuNavigator>, IMenu
{
    [System.NonSerialized] private List<IMenu> _menus = new();
    [SerializeField] private List<Tab> tabs = new();
    [System.NonSerialized] private Data _data;

    
    void Start()
    {
        _menus.Add(BlockMenu.THIS);
        _menus.Add(WeaponMenu.THIS);
    }

    public new void Open(float duration = 1.0f)
    {
        if (base.Open(0.1f))
        {
            return;
        }

        Time.timeScale = 0.0f;
        
        UIManager.THIS.ScaleTransactors(2.0f, true);
        OpenLastMenu();
    }
    
    public new void Close()
    {
        if (base.Close())
        {
            return;
        }
        Time.timeScale = 1.0f;
        UIManager.THIS.ScaleTransactors(1.0f);
        _menus[_data.lastIndex].Close(0.2f);
    }

    private void OpenLastMenu()
    {
        _menus[_data.lastIndex].Open(0.25f);

        for (int i = 0; i < tabs.Count; i++)
        {
            if (i == _data.lastIndex)
            {
                tabs[i].Show();
            }
            else
            {
                tabs[i].Hide();
            }
        }
    }
    
    public void OnTab_BlockMenu()
    {
        _data.lastIndex = 0;
        
        _menus[_data.lastIndex].GetParentContainer().SetAsLastSibling();
        _menus[_data.lastIndex].Open(0.1f);
        _menus[1].Close(0.1f, 0.1f);
        
        tabs[_data.lastIndex].Show();
        tabs[1].Hide();
    }
    
    public void OnTab_WeaponMenu()
    {
        _data.lastIndex = 1;

        _menus[_data.lastIndex].GetParentContainer().SetAsLastSibling();
        _menus[_data.lastIndex].Open(0.1f);
        _menus[0].Close(0.1f, 0.1f);
        
        tabs[_data.lastIndex].Show();
        tabs[0].Hide();
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
        [SerializeField] public int lastIndex = 0;

        public Data()
        {
                
        }
        public Data(Data data)
        {
            lastIndex = data.lastIndex;
        }   
        public object Clone()
        {
            return new Data(this);
        }
    } 
}

