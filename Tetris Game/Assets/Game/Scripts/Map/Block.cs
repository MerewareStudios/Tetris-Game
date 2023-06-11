using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Block : MonoBehaviour
    {
        [SerializeField] public int width;
        [SerializeField] private List<Transform> segmentTransforms;
        [System.NonSerialized] public readonly List<Pawn> Pawns = new();
        [SerializeField] public Vector3 spawnerOffset;
        [System.NonSerialized] private Tween _motionTween;
        [System.NonSerialized] private bool _busy = false;
        [System.NonSerialized] public bool PlacedOnGrid = false;

        public void Construct()
        {
            foreach (var target in segmentTransforms)
            {
                Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, 1);
                pawn.MarkSpawnColor();
                pawn.ParentBlock = this;
                pawn.Show();
                Pawns.Add(pawn);
            }
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
            }
            Spawner.THIS.RemoveBlock(this);

            Deconstruct();
        }

        public void Rotate()
        {
            if (_busy)
            {
                return;
            }
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
            if (_busy)
            {
                return;
            }
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
