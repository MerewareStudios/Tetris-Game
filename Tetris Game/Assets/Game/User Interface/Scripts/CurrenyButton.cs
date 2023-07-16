using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrenyButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI text;

    public CurrenyButton SetAvailable(bool state)
    {
        text.text = state ? "BUY" : "NO FUNDS";
        _button.image.enabled = state;
        return this;
    }
    
    public CurrenyButton SetMax(bool state)
    {
        text.text = state ? "MAX" : "BUY";
        _button.image.enabled = !state;
        return this;
    }
}
