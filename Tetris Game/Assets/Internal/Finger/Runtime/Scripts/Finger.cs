using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace IWI.Tutorial
{
    public class Finger : MonoBehaviour
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private RectTransform canvasRect;

        [SerializeField] private RectTransform tip;
        [SerializeField] private RectTransform positionPivot;
        [SerializeField] private RectTransform scalePivot;
        [SerializeField] private RectTransform offsetPivot;
        [SerializeField] private Vector3 scaleDown;
        [SerializeField] private Vector2 offset;
        [SerializeField] private Vector2 shortDragOffset;
        [SerializeField] private float downDuration = 0.2f;
        [SerializeField] private float upDuration = 0.2f;
        [SerializeField] private float shortDragDuration = 0.3f;
        [SerializeField] private Ease downEase;
        [SerializeField] private Ease upEase;
        [SerializeField] private ParticleSystem ps;
        [System.NonSerialized] private Coroutine _routine;
        [System.NonSerialized] private Sequence _sequence;
        [System.NonSerialized] public System.Action OnClick;
        [System.NonSerialized] public System.Action OnDown;
        [System.NonSerialized] public System.Action OnDrag;
        [System.NonSerialized] public System.Action OnUp;
        
        [System.NonSerialized] public Camera mainCamera;
        [System.NonSerialized] public Camera uiCamera;
        
        void Awake()
        {
            mainCamera = Camera.main;
            uiCamera = canvas.worldCamera;
        }

        public void ShortPressAndDrag(Vector3 position, bool worldSpace, float pressDuration)
        {
            Stop();
            
            canvas.enabled = true;
            positionPivot.gameObject.SetActive(false);
            
            this.WaitForNull(Task);

            void Task()
            {
                positionPivot.gameObject.SetActive(true);

                positionPivot.anchoredPosition = GetAnchor(position, worldSpace);
                
                scalePivot.localScale = Vector3.one;
                offsetPivot.localPosition = Vector3.zero;
                offsetPivot.localRotation = Quaternion.identity;
                
                Tween scaleDownTween = scalePivot.DOScale(scaleDown, downDuration).SetEase(downEase);
                Tween scaleUpTween = scalePivot.DOScale(Vector3.one, upDuration).SetEase(upEase);
                
                Tween offsetDownTween = offsetPivot.DOAnchorPos(offset, downDuration).SetEase(downEase);
                Tween offsetUpTween = offsetPivot.DOAnchorPos(Vector3.zero, upDuration);
                
                Tween dragTween = offsetPivot.DOAnchorPos(shortDragOffset, shortDragDuration).SetRelative(true);

                
                _sequence?.Kill();
                _sequence = DOTween.Sequence();
                _sequence.SetAutoKill(false);
                _sequence.Pause();

                _sequence.Append(scaleDownTween).Join(offsetDownTween).Append(dragTween).Append(scaleUpTween).Join(offsetUpTween);
                
                _sequence.SetUpdate(true);

                scaleDownTween.onComplete = () =>
                {
                    ps.transform.position = tip.position;
                    ps.Emit(1);
                    OnClick?.Invoke();
                };
                
                _routine = StartCoroutine(Loop());

                IEnumerator Loop()
                {
                    yield return new WaitForSecondsRealtime(0.25f); 

                    _sequence.Play();
                    
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(upDuration + downDuration + shortDragDuration + 1.0f); 
                        _sequence.Restart();
                    }
                }
            }
        }
        
        public void Click(Vector3 position, bool worldSpace)
        {
            Stop();
            
            canvas.enabled = true;
            positionPivot.gameObject.SetActive(false);

            this.WaitForNull(Task);

            void Task()
            {
                positionPivot.gameObject.SetActive(true);

                positionPivot.anchoredPosition = GetAnchor(position, worldSpace);
                
                scalePivot.localScale = Vector3.one;
                offsetPivot.localPosition = Vector3.zero;

                Tween scaleDownTween = scalePivot.DOScale(scaleDown, downDuration).SetEase(downEase);
                Tween scaleUpTween = scalePivot.DOScale(Vector3.one, upDuration).SetEase(upEase);
                
                Tween offsetDownTween = offsetPivot.DOAnchorPos(offset, downDuration).SetEase(downEase);
                Tween offsetUpTween = offsetPivot.DOAnchorPos(Vector3.zero, upDuration).SetEase(upEase);

                
                _sequence?.Kill();
                _sequence = DOTween.Sequence();
                _sequence.SetAutoKill(false);
                _sequence.Pause();
                
                _sequence.Append(scaleDownTween).Join(offsetDownTween).Append(scaleUpTween).Join(offsetUpTween);

                _sequence.SetUpdate(true);
                
                scaleDownTween.onComplete = () =>
                {
                    ps.transform.position = tip.position;
                    ps.Emit(1);
                    OnClick?.Invoke();
                };


                _routine = StartCoroutine(Loop());

                IEnumerator Loop()
                {
                    yield return new WaitForSecondsRealtime(0.25f); 

                    _sequence.Play();
                    
                    while (true)
                    {
                        yield return new WaitForSecondsRealtime(upDuration + downDuration + 1.0f); 
                        _sequence.Restart();
                    }
                }
            }

        }
        
        public Vector3 GetAnchor(Vector3 position, bool worldSpace)
        {
            Camera cam = worldSpace ? this.mainCamera : this.uiCamera;
            Vector2 localPoint = cam.WorldToScreenPoint(position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, localPoint, this.uiCamera, out Vector2 local);
            return local;
        }

        public void Hide()
        {
            Stop();
            _sequence?.Kill();
            canvas.enabled = false;
            ps.Clear();
            ps.Stop();
        }
        
        public void Stop()
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }
        }
    }
}
