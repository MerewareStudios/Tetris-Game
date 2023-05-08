using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Visuals.Tutorial
{
    public class Blackout : MonoBehaviour
    {
        public static Blackout instance;
        public static Blackout THIS
        {
            get
            {
                if (instance == null)
                {
                    Instantiate();
                }
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        private static void Instantiate()
        {
            THIS = Instantiate(Resources.Load<Blackout>("Menu/Blackout"));
        }
        

        public static void Show(Camera sceneUICamera, float delay, System.Action onShowComplete)
        {
            THIS.ShowSelf(sceneUICamera, delay, onShowComplete);
        }
        public static void Hide(float duration, System.Action onHideComplete = null)
        {
            THIS.HideSelf(duration, onHideComplete);
        }

        [SerializeField] private Canvas canvas;
        [SerializeField] private RawImage image;
        [System.NonSerialized] private Tween fadeTween;
        public bool Block
        {
            set
            {
                image.raycastTarget = value;
            }
        }

        private void ShowSelf(Camera uiCamera, float delay, System.Action onShowComplete)
        {
            canvas.worldCamera = uiCamera;
            HaltTimedActivities();
            fadeTween = image.GoFade(1.0f, 0.25f, DG.Tweening.Ease.OutSine, true, delay, 0.0f, onShowComplete);
            Block = true;
        }
        private void HideSelf(float duration, System.Action onHideComplete)
        {
            Block = false;
            HaltTimedActivities();
            fadeTween = image.GoFade(0.0f, duration, DG.Tweening.Ease.InOutSine, true, 0.1f, null, () => { DestroySelf(); onHideComplete?.Invoke(); } );
        }
        public void DestroySelf()
        {
            HaltTimedActivities();
            Destroy(THIS.gameObject);
            THIS = null;
        }
        private void HaltTimedActivities()
        {
            fadeTween?.Kill();
        }
    }

}