using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerSelection : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;

    public void Set(System.Action<int> onClick, int powerIndex)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick.Invoke(powerIndex));
    }
    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
    }
}
