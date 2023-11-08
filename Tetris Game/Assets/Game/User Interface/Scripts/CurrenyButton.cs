using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrenyButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI text;
    [System.NonSerialized] private bool _available = true;

    public bool Available
    {
        set
        {
            _available = value;
            _button.targetGraphic.color = value ? Const.THIS.currenyButtonNormalColor : Const.THIS.currenyButtonFadedColor;
            text.color = text.color.SetAlpha(value ? 1.0f : 0.5f);
        }
        get => _available;
    }
    public Sprite ButtonSprite
    {
        set => _button.image.sprite = value;
    }
    public CurrenyButton SetButton(string buttonStr, bool imageEnabled)
    {
        text.text = buttonStr;
        _button.image.enabled = imageEnabled;
        return this;
    }
}
