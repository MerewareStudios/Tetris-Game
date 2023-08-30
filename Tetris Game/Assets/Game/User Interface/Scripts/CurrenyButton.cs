using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrenyButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI text;

    public bool Interactable
    {
        set => _button.interactable = value;
    } 
    
    public CurrenyButton SetAvailable(bool state)
    {
        text.text = state ? "BUY" : "NO FUNDS";
        _button.image.enabled = state;
        return this;
    }
    
    public CurrenyButton SetAvailable(bool state, string statement)
    {
        text.text = state ? statement : "NO FUNDS";
        _button.image.enabled = state;
        return this;
    }
    
    public CurrenyButton SetFull(bool state)
    {
        text.text = state ? Onboarding.THIS.fullText : "BUY";
        _button.image.enabled = !state;
        return this;
    }
    
    public CurrenyButton Set(string str, bool buttonImageActive)
    {
        text.text = str;
        _button.image.enabled = buttonImageActive;
        return this;
    }
}
