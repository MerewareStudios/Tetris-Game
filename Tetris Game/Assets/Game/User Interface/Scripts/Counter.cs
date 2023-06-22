using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace  Internal.Visuals
{
    public class Counter : Singleton<Counter>
    {

        [SerializeField] private RectTransform animationPivot;
        [SerializeField] public Image fill;
        [SerializeField] private TextMeshProUGUI text;
        
        public void Value(int current, float max)
        {
            text.text = current.ToString();
            fill.fillAmount = current / max;
        }
        public void ValueAnimated(int current, float max, float punch)
        {
            Value(current, max);
            Punch(punch);
        }

        private void Punch(float amount)
        {
            animationPivot.DOKill();
            animationPivot.localScale = Vector3.one;
            animationPivot.DOPunchScale(Vector3.one * amount, 0.25f);
        }
    }
}
