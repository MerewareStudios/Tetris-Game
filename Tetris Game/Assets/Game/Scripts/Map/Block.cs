using System;
using DG.Tweening;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game
{
    public class Block : MonoBehaviour
    {
        [SerializeField] public int width;
        [SerializeField] private List<Transform> segmentTransforms;
        [System.NonSerialized] public readonly List<Pawn> Pawns = new();
        [SerializeField] public Vector3 spawnerOffset;
        [System.NonSerialized] private Tween _motionTween;
        [System.NonSerialized] public bool _busy = false;
        [System.NonSerialized] public bool PlacedOnGrid = false;

        private void OnDrawGizmos()
        {
            foreach (var p in segmentTransforms)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(p.position, Vector3.one * 0.9f);
            }
        }

        public void Construct()
        {
            foreach (var target in segmentTransforms)
            {
                Pawn.Usage usage = Pawn.Usage.Ammo;
                Helper.IsPossible(0.025f, () => OverrideUsage(out usage));
                Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, 1, usage);
                pawn.ParentBlock = this;
                pawn.MarkDefaultColor();
                pawn.Show();
                Pawns.Add(pawn);
            }
        }

        private void OverrideUsage(out Pawn.Usage usage)
        {
            usage = Const.THIS.PowerUps.Random();
        }

        public void Deconstruct()
        {
            if (!PlacedOnGrid)
            {
                foreach (var pawn in Pawns)
                {
                    pawn.Deconstruct();
                }
            }
            _motionTween?.Kill();
            _busy = false;
            Pawns.Clear();
            PlacedOnGrid = false;
            
            this.Despawn();
        }
        public void Detach()
        {
            foreach (var pawn in Pawns)
            {
                pawn.ParentBlock = null;
                pawn.MOVER = false;
                pawn.MarkSteadyColor();
            }
            Spawner.THIS.RemoveBlock(this);

            Deconstruct();
        }

        public void Rotate()
        {
            _busy = true;

            _motionTween?.Kill();
            _motionTween = transform.DORotate(new Vector3(0.0f, 90.0f, 0.0f), Const.THIS.rotationDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Const.THIS.rotationEase);
            _motionTween.onUpdate += () => 
                {
                    foreach (var segment in Pawns)
                    {
                        segment.transform.rotation = Quaternion.identity;
                    }
                };
            _motionTween.onComplete += () =>
            {
                _busy = false;
            };
        }
        public void Move(Vector3 position, float duration, Ease ease, bool speedBased = false)
        {
            _busy = true;

            _motionTween?.Kill();
            _motionTween = transform.DOMove(position, duration).SetEase(ease).SetSpeedBased(speedBased);
            _motionTween.onComplete += () =>
            {
                _busy = false;
            };
        }

    }
}
