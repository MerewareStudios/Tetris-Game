using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;
using Febucci.UI;

namespace  Game.UI
{
    public class Announcer : Singleton<Announcer>
    {
        [SerializeField] private RectTransform textRect;
        [SerializeField] private Canvas canvas;
        [System.NonSerialized] private Coroutine countdownRoutine;
        [System.NonSerialized] private Sequence _sequence;
        public TypewriterByCharacter animatedText;

        public Coroutine Show(string str, float stayDuration, System.Action onCountDown)
        {
            canvas.enabled = true;
            countdownRoutine = StartCoroutine(CountRoutine());
                
            IEnumerator CountRoutine()
            {
                onCountDown?.Invoke();
                float dur = ShowText(str, stayDuration);
                yield return new WaitForSeconds(dur);
                Stop();
            }

            return countdownRoutine;
        }

        public Coroutine Count(string startingText, int seconds, System.Action<int> onCountDown)
        {
            canvas.enabled = true;
            countdownRoutine = StartCoroutine(CountRoutine());
                
            IEnumerator CountRoutine()
            {
                // ShowText(startingText, 0.7f);
                // yield return new WaitForSeconds(1.4f);
                for (int i = 0; i < seconds; i++)
                {
                    onCountDown?.Invoke(i);
                    ShowText((seconds - i).ToString());
                    yield return new WaitForSeconds(0.85f);
                }
            }

            return countdownRoutine;
        }

        private float ShowText(string str, float stayDuration = 0.2f)
        {
            animatedText.ShowText(str);

            textRect.localScale = Vector3.zero;
            Tween scaleUp = textRect.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
            Tween scaleDown = textRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCirc).SetDelay(stayDuration);

            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.Append(scaleUp);
            _sequence.Append(scaleDown);

            return _sequence.Duration();
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
