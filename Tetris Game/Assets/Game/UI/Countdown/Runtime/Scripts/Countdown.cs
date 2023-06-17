using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

namespace  Game.UI
{
    public class Countdown : Singleton<Countdown>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform textRect;
        [SerializeField] private Canvas canvas;
        [System.NonSerialized] private Coroutine countdownRoutine;
        [System.NonSerialized] private Sequence _sequence;



        public void Count(int seconds)
        {
            canvas.enabled = true;
            countdownRoutine = StartCoroutine(CountRoutine());
                
            IEnumerator CountRoutine()
            {
                for (int i = 0; i < seconds; i++)
                {
                    text.text = (seconds - i).ToString();

                    textRect.localScale = Vector3.zero;
                    Tween scaleUp = textRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
                    Tween scaleDown = textRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCirc).SetDelay(0.3f);

                    _sequence = DOTween.Sequence();
                    _sequence.Append(scaleUp);
                    _sequence.Append(scaleDown);

                    yield return new WaitForSeconds(1);
                }
            }
        }

        public void Stop()
        {
            if (countdownRoutine != null)
            {
                StopCoroutine(countdownRoutine);
                countdownRoutine = null;
            }

            canvas.enabled = false;
            textRect.DOKill();
            _sequence?.Kill();
        }
    }
}
