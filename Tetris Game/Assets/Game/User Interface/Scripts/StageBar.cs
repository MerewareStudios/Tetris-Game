using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageBar : MonoBehaviour
{
    [SerializeField] private Color[] barColors; // Empty, Locked, Upgraded
    [SerializeField] private Image[] images;
    [SerializeField] private CurrenyButton purchaseButton;
    [SerializeField] private CurrencyDisplay currencyDisplay;
    [SerializeField] public RectTransform clickTarget;
    
    public bool Available
    {
        set => purchaseButton.Available = value;
        get => purchaseButton.Available;
    }


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
    // public StageBar SetInteractable(bool state)
    // {
    //     purchaseButton.Available = state;
    //     return this;
    // }
    public StageBar SetMaxed(bool state)
    {
        purchaseButton.SetButton(state ? Onboarding.THIS.plusText : Onboarding.THIS.fullText, state);
        return this;
    }
    public StageBar SetBars(int availableCount, int filledCount, int alreadyHaveCount)
    {
        for (int i = 0; i < images.Length; i++)
        {
            if (i + 1 <= alreadyHaveCount)
            {
                SetBar(i, BarStat.Have);
                continue;
            }
            if (i + 1 <= filledCount + alreadyHaveCount)
            {
                SetBar(i, BarStat.Upgraded);
                continue;
            }
            if (i + 1 <= availableCount + alreadyHaveCount)
            {
                SetBar(i, BarStat.Empty);
                continue;
            }
            SetBar(i, BarStat.Locked);
        }

        return this;
    }
    
    private void SetBar(int index, BarStat barStat)
    {
        images[index].color = barColors[(int)barStat];
    }
    
    public void PunchPurchaseButton(float amount)
    {
        Transform buttonTransform = purchaseButton.transform;
        buttonTransform.DOKill();
        buttonTransform.localEulerAngles = Vector3.zero;
        buttonTransform.localScale = Vector3.one;
        buttonTransform.DOPunchScale(Vector3.one * amount, 0.3f, 1).SetUpdate(true);
    }

    [Serializable]
    public enum BarStat
    {
        Empty,
        Locked,
        Upgraded,
        Have
    }
    
}
