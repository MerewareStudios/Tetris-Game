using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

namespace  Game.UI
{
    public class Announcer : Singleton<Announcer>
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private RectTransform textRect;
        [SerializeField] private Canvas canvas;
        [System.NonSerialized] private Coroutine countdownRoutine;
        [System.NonSerialized] private Sequence _sequence;



        public void Count(int levelIndex, int seconds)
        {
            canvas.enabled = true;
            countdownRoutine = StartCoroutine(CountRoutine());
                
            IEnumerator CountRoutine()
            {
                ShowText("Wave " + levelIndex);
                yield return new WaitForSeconds(1);
                for (int i = 0; i < seconds; i++)
                {
                    ShowText((seconds - i).ToString());
                    yield return new WaitForSeconds(1);
                }
            }
        }

        private void ShowText(string str)
        {
            text.text = str;

            textRect.localScale = Vector3.zero;
            Tween scaleUp = textRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
            Tween scaleDown = textRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCirc).SetDelay(0.3f);

            _sequence = DOTween.Sequence();
            _sequence.Append(scaleUp);
            _sequence.Append(scaleDown);
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
