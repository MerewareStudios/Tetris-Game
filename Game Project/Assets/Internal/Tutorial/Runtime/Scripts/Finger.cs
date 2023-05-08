using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Internal.Visuals.Tutorial
{
    public class Finger : MonoBehaviour
    {
        public static Finger currentReference;
        public static Finger THIS
        {
            get
            {
                if (currentReference == null)
                {
                    Instantiate();
                }
                return currentReference;
            }
            set
            {
                currentReference = value;
            }
        }

        private static void Instantiate()
        {
            THIS = Instantiate(Resources.Load<Finger>("Menu/Finger"));
        }


        public static Finger Show(Camera sceneUICamera, GetTargetPosition OnGetTargetPosition, float releaseDelay)
        {
            THIS.canvas.worldCamera = sceneUICamera;
            THIS.canvas.sortingLayerName = "Overlay";
            THIS.canvas.sortingOrder = 7;
            THIS.fadeTween?.Kill();
            THIS.StopAnimation();
            THIS.KillChildTweens();
            THIS.OnGetTargetPosition = OnGetTargetPosition;
            THIS.ShowFinger();
            THIS.PlayAnimation(0.1f, releaseDelay);
            return THIS;
        }
        public static void Hide()
        {
            THIS.DestroySelf();
        }

        [SerializeField] private Canvas canvas;
        [SerializeField] private Transform fingerPositionPivot;
        [SerializeField] private Transform fingerScalePivot;
        [SerializeField] private Transform fingerLocalPivot;
        [SerializeField] private Transform fingerRotationPivot;
        [SerializeField] private ParticleSystem fingerPS;
        [SerializeField] private Transform fingerPSLoc;
        [SerializeField] private Image fingerImage;
        [SerializeField] private TextMeshProUGUI infoText;
        [System.NonSerialized] private Coroutine coroutine;
        [System.NonSerialized] private Tween fadeTween;
        public delegate Vector3? GetTargetPosition();
        private GetTargetPosition OnGetTargetPosition;

        void LateUpdate()
        {
            Vector3? targetPos = OnGetTargetPosition?.Invoke();
            if (targetPos == null)
            {
                return;
            }
            fingerPositionPivot.position = (Vector3)targetPos; 
        }
        internal void SetText(string info, Vector3 offset)
        {
            if (info == null)
            {
                return;
            }
            infoText.text = info;
            infoText.transform.localPosition = offset;
        }
        private void PlayAnimation(float startDelay, float releaseDelay)
        {
            IEnumerator PressAndRelease()
            {
                yield return new WaitForSecondsRealtime(startDelay);

                while (true)
                {
                    PressAnimation();

                    yield return new WaitForSecondsRealtime(releaseDelay);

                    ReleaseAnimation();

                    yield return new WaitForSecondsRealtime(1.0f);
                }
            }
            StopAnimation();
            coroutine = StartCoroutine(PressAndRelease());
        }
        private void StopAnimation()
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
        }
        private void KillChildTweens()
        {
            fingerScalePivot.DOKill();
            fingerLocalPivot.DOKill();
            fingerRotationPivot.DOKill();
        }
        private void PressAnimation()
        {
            KillChildTweens();
            fingerScalePivot.DOScale(Vector3.one * 0.75f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete += () => { Emit(); };
            fingerLocalPivot.DOLocalMove(new Vector3(30.0f, 0.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
            fingerRotationPivot.DOLocalRotate(new Vector3(0.0f, 0.0f, 10.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
        }
        private void ReleaseAnimation()
        {
            KillChildTweens();
            fingerScalePivot.DOScale(Vector3.one * 1.0f, 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
            fingerLocalPivot.DOLocalMove(new Vector3(0.0f, -70.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
            fingerRotationPivot.DOLocalRotate(new Vector3(0.0f, 0.0f, 0.0f), 0.125f).SetEase(Ease.InOutSine).SetUpdate(true);
        }
        private void Emit()
        {
            fingerPS.transform.position = fingerPSLoc.position;
            fingerPS.Emit(1);
        }
        private void ShowFinger()
        {
            fadeTween?.Kill();
            fadeTween = fingerImage.GoFade(1.0f, 0.25f, DG.Tweening.Ease.InOutSine, true, 0.0f, 0.0f, null);
        }
        public void DestroySelf()
        {
            fadeTween?.Kill();
            StopAnimation();
            KillChildTweens();
            Destroy(THIS.gameObject);
            THIS = null;
        }
    }

}