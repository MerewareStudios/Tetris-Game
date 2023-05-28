using DG.Tweening;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private List<Transform> segmentTransforms;
        [System.NonSerialized] private List<Segment> segments = new();
        [SerializeField] public Vector3 spawnerOffset;
        [System.NonSerialized] private Tween motionTween;
        [System.NonSerialized] private bool busy = false;

        public bool CanMoveForward{get; set;}

        public void Construct()
        {
            this.transform.localPosition = spawnerOffset;
            this.transform.localScale = Vector3.one;
            this.transform.localRotation = Quaternion.identity;

            foreach (var s in segmentTransforms)
            {
                Segment segment = Pool.Segment___Level_1.Spawn<Segment>(this.transform);
                segment.SetLevel(1);
                segment.transform.position = s.position;
                segment.transform.localRotation = Quaternion.identity;
                segment.transform.localScale = Vector3.one;
                segment.parentBlock = this;
                segment.SetDeckColor();
                segments.Add(segment);
            }

            CanMoveForward = false;
        }

        public void Deconstruct()
        {
            foreach (var segment in segments)
            {
                segment.Mover = false;
                segment.parentBlock = null;
            }
            segments.Clear();
            this.Despawn();
        }

        public bool SubmitToPlaces()
        {
            foreach (var segment in segments)
            {
                if (!segment.CanSubmit)
                {
                    return false;
                }
            }
            foreach (var segment in segments)
            {
                segment.Submit();
            }
            return true;
        }

        public void Check()
        {
            foreach (var segment in segments)
            {
                segment.Check();
            }
        }

        public void Rotate()
        {
            if (busy)
            {
                return;
            }
            busy = true;

            motionTween?.Kill();
            motionTween = transform.DORotate(new Vector3(0.0f, 90.0f, 0.0f), 0.2f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.Linear);
            motionTween.onUpdate += () => 
                {
                    foreach (var segment in segments)
                    {
                        segment.transform.rotation = Quaternion.identity;
                    }
                };
            motionTween.onComplete += () =>
            {
                busy = false;
            };
        }

        public void Mount()
        {
            busy = true;

            motionTween?.Kill();
            motionTween = transform.DOLocalMove(spawnerOffset, GameManager.THIS.Constants.segmentDenyDuration).SetEase(GameManager.THIS.Constants.segmentDenyEase);
            motionTween.onComplete += () =>
            {
                busy = false;
            };
        }

        private void OnDrawGizmos()
        {
            foreach (var s in segmentTransforms)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(s.transform.position, Vector3.one * 0.95f * transform.localScale.x);
            }
        }

        public enum Type
        {
            I,
            J,
            L,
            O,
            S,
            T,
            Z
        }
    }
}
