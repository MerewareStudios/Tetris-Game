using System;
using System.Text;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageBar : MonoBehaviour
{
    [SerializeField] private Color[] barColors; // Empty, Locked, Upgraded
    [SerializeField] private Image[] images;
    [SerializeField] private CurrenyButton purchaseButton;
    [SerializeField] private CurrencyDisplay currencyDisplay;


    public StageBar SetCurrencyStampVisible(bool state)
    {
        currencyDisplay.gameObject.SetActive(state);
        return this;
    }
    public StageBar SetPrice(Const.Currency currency)
    {
        currencyDisplay.Display(currency);
        return this;
    }
    public StageBar SetInteractable(bool state)
    {
        purchaseButton.Interactable = state;
        return this;
    }
    public StageBar SetMaxed(bool state)
    {
        purchaseButton.Set(state ? Onboarding.THIS.plusText : Onboarding.THIS.fullText, state);
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
