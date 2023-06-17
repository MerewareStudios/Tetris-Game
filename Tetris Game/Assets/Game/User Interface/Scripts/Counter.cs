using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace  Internal.Visuals
{
    public class Counter : Singleton<Counter>
    {

        [SerializeField] private RectTransform animationPivot;
        [SerializeField] private Image fill;
        [SerializeField] private TextMeshProUGUI text;
        
        public void Value(int current, float max)
        {
            text.text = current.ToString();
            fill.fillAmount = current / max;
        }
        public void ValueAnimated(int current, float max)
        {
            text.text = current.ToString();
            fill.fillAmount = current / max;
        }
    }
}
