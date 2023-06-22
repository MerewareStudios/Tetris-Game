using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Game.UI;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Internal.Core;

public class StageBar : MonoBehaviour
{
    [SerializeField] private StatDisplay.Type statType;
    [SerializeField] private TextMeshProUGUI topInfoText;
    [SerializeField] private Color[] barColors; // Empty, Locked, Upgraded
    [SerializeField] private Image[] images;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button[] purchaseButtons;
    [SerializeField] private GameObject buttonParent;


    public StageBar SetTopInfo(string value)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(statType.ToString().ToUpper()).Append("\n").Append(statType.ToTMProKey()).Append(value);
        topInfoText.text = stringBuilder.ToString();
        return this;
    }
 
    public StageBar SetPrice(int amount)
    {
        if (amount == 0)
        {
            priceText.text = "MAX";
            return this;
        }
        if (amount == -1)
        {
            priceText.text = "AD".ToTMProKey() + " FREE";
            return this;
        }
        priceText.text = "COIN".ToTMProKey() + amount;
        return this;
    }
    
    public StageBar SetPurchaseType(Game.UI.PurchaseType purchaseType)
    {
        for (int i = 0; i < purchaseButtons.Length; i++)
        {
            Button button = purchaseButtons[i];
            button.gameObject.SetActive((int)purchaseType == i);
        }

        return this;
    }
    
    public StageBar SetBars(int lockedCount, int upgradedCount)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i <= upgradedCount)
            {
                SetBar(i, BarStat.Upgraded);
            }
            else if (i <= lockedCount)
            {
                SetBar(i, BarStat.Empty);
            }
            else
            {
                SetBar(i, BarStat.Locked);
            }
        }

        return this;
    }
    
    public StageBar SetUsable(bool usable)
    {
        buttonParent.SetActive(usable);
        return this;
    }
    

    
    private void SetBar(int index, BarStat barStat)
    {
        images[index].color = barColors[(int)barStat];
    }

    [Serializable]
    public enum BarStat
    {
        Empty,
        Locked,
        Upgraded
    }
    
    [Serializable]
    public class StageData<T> : ICloneable
    {
        [SerializeField] public Game.UI.PurchaseType purchaseType;
        [SerializeField] public T value;
        [SerializeField] public int price;
            
        public StageData()
        {
                
        }
        public StageData(StageData<T> gunStatUpgradeData)
        {
                
        }
        public object Clone()
        {
            return new StageData<T>(this);
        }
    }
}
