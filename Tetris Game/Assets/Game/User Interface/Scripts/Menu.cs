using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class Menu<T> : Singleton<T> where T : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image _blocker;
        [System.NonSerialized] private Tween showTween;

        public virtual Menu<T> Open()
        {
            Time.timeScale = 0.0f;
            canvas.enabled = true;
            canvasGroup.alpha = 0.0f;
            _blocker.raycastTarget = true;
            showTween?.Kill();
            showTween = canvasGroup.DoFade_IWI(1.0f, 0.5f, Ease.InOutSine, () =>
            {
                _blocker.raycastTarget = false;
            }).SetUpdate(true);
            return this;
        }
        
        public virtual Menu<T> Close()
        {
            _blocker.raycastTarget = true;
            showTween?.Kill();
            showTween = canvasGroup.DoFade_IWI(0.0f, 0.25f, Ease.InOutSine, () =>
            {
                canvas.enabled = false;  
                Time.timeScale = 1.0f;
            }).SetUpdate(true);
            return this;
        }
    }
}
