using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrenyButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI text;

    public bool Available
    {
        set
        {
            _button.targetGraphic.color =
                value ? Const.THIS.currenyButtonNormalColor : Const.THIS.currenyButtonFadedColor;
            text.color = text.color.SetAlpha(value ? 1.0f : 0.5f);
        }
    }
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
