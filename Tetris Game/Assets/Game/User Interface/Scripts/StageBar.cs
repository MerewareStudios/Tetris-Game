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

public class StageBar : MonoBehaviour
{
    [SerializeField] private StatDisplay.Type statType;
    [SerializeField] private TextMeshProUGUI topInfoText;
    [SerializeField] private Color[] barColors; // Empty, Locked, Upgraded
    [SerializeField] private Image[] images;
    [SerializeField] private CurrencyDisplay priceCurrencyDisplay;
    [SerializeField] private CurrenyButton purchaseButton;


    public StageBar SetTopInfo(string value)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append(statType.ToString().ToUpper()).Append("\n").Append(statType.ToTMProKey()).Append(value);
        topInfoText.text = stringBuilder.ToString();
        return this;
    }
 
    public StageBar SetPrice(Const.Currency currency, bool max)
    {
        priceCurrencyDisplay.gameObject.SetActive(!max);

        if (max)
        {
            purchaseButton.SetMax(max);
            return this;
        }
        bool hasFunds = Wallet.HasFunds(currency);

        purchaseButton.SetAvailable(hasFunds);
        
        priceCurrencyDisplay.Display(currency);
        
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
        [SerializeField] public Const.Currency currency;
        [SerializeField] public T value;
            
        public StageData()
        {
                
        }
        public StageData(StageData<T> gunStatUpgradeData)
        {
            this.currency = gunStatUpgradeData.currency;
            this.value = gunStatUpgradeData.value;
        }
        public object Clone()
        {
            return new StageData<T>(this);
        }
    }
}
