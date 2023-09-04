using System;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using Febucci.UI.Core;
using Internal.Core;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Game
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private Transform shakePivot;
        [SerializeField] public int FitHeight;
        [SerializeField] public int NormalHeight;
        [SerializeField] public int NormalWidth;
        [SerializeField] public int[] checkAngles;
        [SerializeField] private List<Transform> segmentTransforms;
        [SerializeField] public Vector3 spawnerOffset;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] public  BlockData blockData;
        
        [System.NonSerialized] public readonly List<Pawn> Pawns = new();
        [System.NonSerialized] private Tween _motionTween;
        [System.NonSerialized] public bool Busy = false;
        [System.NonSerialized] public bool PlacedOnGrid = false;
        [System.NonSerialized] public List<int> RequiredIndexes;
        [System.NonSerialized] public bool canRotate;

        private void OnDrawGizmos()
        {
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 4; y++)
                {
                    Gizmos.color = new Color(1.0f, 0.2f, 0.2f, 0.5f);
                    Gizmos.DrawCube( transform.position + new Vector3(x - 1.0f, 0.0f, y - 1.5f), Vector3.one * 0.95f);
                }
            }
        
            foreach (var segmentTransform in segmentTransforms)
            {
                if (segmentTransform)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawCube(segmentTransform.position, Vector3.one * 0.9f);
                }
            }
            
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(rotatePivot.position, 0.1f);
        }

        public List<Vector3> LocalPawnPositions => (from segmentTransform in segmentTransforms where segmentTransform select segmentTransform.localPosition).ToList();

        public Board.BlockRot Rotation
        {
            set
            {
                rotatePivot.localRotation = Quaternion.Euler(0.0f, 90.0f * (int)value, 0.0f);
                ResetSegmentRotations();
            }
        }

        public void Construct(Pawn.Usage usage) 
        {
            for (int i = 0; i < segmentTransforms.Count; i++)
            {
                Transform target = segmentTransforms[i];
                if (!target) continue;
                
                
                if (usage.Equals(Pawn.Usage.Ammo) && LevelManager.THIS.CanSpawnBonus())
                {
                    Helper.IsPossible(0.025f, () => OverrideUsage(out usage));
                }
                
                
                Pawn pawn = Spawner.THIS.SpawnPawn(this.rotatePivot, target.position, 1, usage);
                pawn.ParentBlock = this;
                pawn.MarkDefaultColor();
                pawn.Show();
                Pawns.Add(pawn);
            }
        }

        public void OnPickUp()
        {
            // if (FreeBlock)
            // {
            //     Board.THIS.ShowAvailablePlaces();
            // }
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
            RequiredIndexes = null;
            this.Despawn();
        }
        public void DeconstructAnimated()
        {
            if (!PlacedOnGrid)
            {
                foreach (var pawn in Pawns)
                {
                    pawn.DeconstructAnimated(false);
                }
            }
            _motionTween?.Kill();
            Busy = false;
            Pawns.Clear();
            PlacedOnGrid = false;
            RequiredIndexes = null;
            this.Despawn();
        }
        public void Detach()
        {
            foreach (var pawn in Pawns)
            {
                pawn.ParentBlock = null;
                pawn.MOVER = false;
                pawn.MarkSteadyColor();
                pawn.PunchUp(-0.125f, 0.25f);
            }
            Spawner.THIS.RemoveBlock(this);

            Deconstruct();
        }

        public void Shake()
        {
            shakePivot.DOKill();
            shakePivot.localPosition = Vector3.zero;
            shakePivot.localEulerAngles = Vector3.zero;
            shakePivot.DOPunchRotation(new Vector3(0.0f, 15.0f, 0.0f), 0.4f, 1);
        }
        public void Lift()
        {
            _motionTween?.Kill();
            
            // shakePivot.DOKill();
            // shakePivot.localPosition = Vector3.zero;
            // shakePivot.localEulerAngles = Vector3.zero;
            _motionTween = transform.DOPunchPosition(new Vector3(0.0f, 2.0f, 0.0f), 2.0f, 1);
        }

        public void CancelLift()
        {
            _motionTween?.Kill();
            // shakePivot.localPosition = Vector3.zero;
            // shakePivot.localEulerAngles = Vector3.zero;
            // shakePivot.DOLocalMove(Vector3.zero, 0.35f).SetEase(Ease.InOutSine);
        }
        public void Rotate()
        {
            if (!canRotate)
            {
                return;
            }
            Busy = true;

            _motionTween?.Kill();
            _motionTween = rotatePivot.DORotate(new Vector3(0.0f, 90.0f, 0.0f), 0.125f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Const.THIS.rotationEase);
            _motionTween.onUpdate = ResetSegmentRotations;
            _motionTween.onComplete += () =>
            {
                Busy = false;
            };
        }

        private void ResetSegmentRotations()
        {
            foreach (var segment in Pawns)
            {
                segment.transform.rotation = Quaternion.identity;
            }
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
