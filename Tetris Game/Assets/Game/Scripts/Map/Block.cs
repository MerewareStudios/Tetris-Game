using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
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
        [System.NonSerialized] public bool Busy = false;
        [System.NonSerialized] public bool PlacedOnGrid = false;
        [System.NonSerialized] public bool FreeBlock = false;

        private void OnDrawGizmos()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.5f);
                    Gizmos.DrawCube(new Vector3(x - 1.0f, 0.0f, y - 1.5f), Vector3.one * 0.95f);
                }
            }

            foreach (var segmentTransform in segmentTransforms.Where(segmentTransform => segmentTransform))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawCube(segmentTransform.position, Vector3.one * 0.9f);
            }
        }

        public void Construct(Pool pool)
        {
            FreeBlock = false;
            
            int[] lookUps = this.GetLookUp(pool);
            for (int i = 0; i < segmentTransforms.Count; i++)
            {
                Transform target = segmentTransforms[i];
                if (!target) continue;
                
                Pawn.Usage usage = Pawn.Usage.Ammo;
                Helper.IsPossible(0.025f, () => OverrideUsage(out usage));
                Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, lookUps[i], usage);
                pawn.ParentBlock = this;
                pawn.MarkDefaultColor();
                pawn.Show();
                Pawns.Add(pawn);
            }
        }
        
        public void Construct(Pool pool, Pawn.Usage usage) 
        {
            FreeBlock = true;

            int[] lookUps = this.GetLookUp(pool);
            for (int i = 0; i < segmentTransforms.Count; i++)
            {
                Transform target = segmentTransforms[i];
                if (!target) continue;
                
                Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, lookUps[i], usage);
                pawn.ParentBlock = this;
                pawn.MarkDefaultColor();
                pawn.Show();
                Pawns.Add(pawn);
            }
        }

        public void OnPickUp()
        {
            if (FreeBlock)
            {
                Board.THIS.ShowAvailablePlaces();
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
            Busy = false;
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
            Busy = true;

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
                Busy = false;
            };
        }
        public void Move(Vector3 position, float duration, Ease ease, bool speedBased = false)
        {
            Busy = true;

            _motionTween?.Kill();
            _motionTween = transform.DOMove(position, duration).SetEase(ease).SetSpeedBased(speedBased);
            _motionTween.onComplete += () =>
            {
                Busy = false;
            };
        }

    }
}
