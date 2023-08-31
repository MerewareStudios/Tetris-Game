using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrenyButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI text;

    public bool Available
    {
        set => _button.targetGraphic.color = value ? Const.THIS.currenyButtonNormalColor : Const.THIS.currenyButtonFadedColor;
    } 
    
    // public CurrenyButton SetAvailable(bool state)
    // {
    //     text.text = state ? "BUY" : "NO FUNDS";
    //     _button.image.enabled = state;
    //     return this;
    // }
    
    // public CurrenyButton SetAvailable(bool state, string statement)
    // {
    //     text.text = state ? statement : "NO FUNDS";
    //     _button.image.enabled = state;
    //     return this;
    // }
    //
    // public CurrenyButton SetFull(bool state)
    // {
    //     text.text = state ? Onboarding.THIS.fullText : "BUY";
    //     _button.image.enabled = !state;
    //     return this;
    // }
    //
    public Sprite ButtonSprite
    {
        set => _button.image.sprite = value;
    }
    public CurrenyButton Set(string str, bool buttonImageActive)
    {
        text.text = str;
        _button.image.enabled = buttonImageActive;
        return this;
    }
}
