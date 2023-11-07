using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerSelection : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image icon;
    [System.NonSerialized] public int PowerIndex;

    public void Set(System.Action<PowerSelection> onClick, Sprite sprite, int powerIndex)
    {
        icon.sprite = sprite;
        PowerIndex = powerIndex;
        
        button.onClick.AddListener(() => onClick.Invoke(this));
    }

}
