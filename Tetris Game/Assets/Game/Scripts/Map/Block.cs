using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private Transform shakePivot;
        [SerializeField] public List<Transform> segmentTransforms;
        [SerializeField] private Transform rotatePivot;
        [SerializeField] public  BlockData blockData;
        
        [System.NonSerialized] private int _currentRotation;
        [System.NonSerialized] private Tween _motionTween;
        
        [System.NonSerialized] public List<Place> RequiredPlaces;
        [System.NonSerialized] public readonly List<Pawn> Pawns = new();
        
        [System.NonSerialized] public bool Busy = false;
        [System.NonSerialized] public bool CanRotate;

        private Pawn PivotPawn => Pawns[0];
        [System.NonSerialized] public Vector2Int UnsafePivotIndex;
        public bool IsPivotPawn(Pawn pawn) => pawn.Equals(PivotPawn);

        public Vector2Int GetUnsafeIndex(Pawn pawn)
        {
            Vector3 dif = pawn.transform.position - PivotPawn.transform.position;
            return UnsafePivotIndex + new Vector2Int(Mathf.RoundToInt(dif.x), -Mathf.RoundToInt(dif.z));
        }

        public List<Vector3> LocalPawnPositions => (from segmentTransform in segmentTransforms where segmentTransform select segmentTransform.localPosition).ToList();
        public Color Color => blockData.Color;

        public Board.BlockRot Rotation
        {
            set
            {
                _currentRotation = (int)value;
                rotatePivot.localRotation = Quaternion.Euler(0.0f, 90.0f * _currentRotation, 0.0f);
                ResetSegmentRotations();
            }
            get => (Board.BlockRot)(_currentRotation % 4);
        }
        
        
        public void Construct(Pool pool, Pawn.Usage usage)
        {
            Block mimicBlock = pool.Prefab<Block>();
            this.blockData = mimicBlock.blockData;
            this.segmentTransforms = mimicBlock.segmentTransforms;    
            
            Transform thisTransform = transform;
            thisTransform.localScale = Vector3.one;
            thisTransform.localPosition = mimicBlock.blockData.spawnerOffset;
            
            this.rotatePivot.localPosition = mimicBlock.rotatePivot.localPosition;
            this.rotatePivot.localEulerAngles = Vector3.zero;

            for (int i = 0; i < segmentTransforms.Count; i++)
            {
                Transform target = segmentTransforms[i];
                if (!target) continue;

                Pawn pawn = Spawner.THIS.SpawnPawn(this.shakePivot, thisTransform.position + target.localPosition, usage.ExtraValue(), usage);
                pawn.ParentBlock = this;

                pawn.Show();
                Pawns.Add(pawn);
            }
        }

        public void Deconstruct()
        {
            foreach (var pawn in Pawns)
            {
                pawn.Deconstruct();
            }
            _motionTween?.Kill();
            Busy = false;
            Pawns.Clear();
            RequiredPlaces = null;
            this.Despawn(Pool.Block);
        }
        public void Detach()
        {
            foreach (var pawn in Pawns)
            {
                pawn.ParentBlock = null;
            }
            Pawns.Clear();
            Spawner.THIS.RemoveBlock(this);

            Deconstruct();
        }
        
        public void DetachPawn(Pawn pawn)
        {
            Pawns.Remove(pawn);
            pawn.ParentBlock = null;
            if (Pawns.Count == 0)
            {
                Spawner.THIS.RemoveBlock(this);
                Deconstruct();
            }
        }

        private void ResetSelf()
        {
            _motionTween?.Kill();
            rotatePivot.localEulerAngles = new Vector3(0.0f, _currentRotation * 90.0f, 0.0f);
            ResetSegmentRotations();
        }
        
        public void ResetRotations()
        {
            ResetSelf();
            ResetSegmentRotations();
        } 

        public void Rotate()
        {
            Busy = true;
            
            shakePivot.DOKill();
            shakePivot.localEulerAngles = Vector3.zero;

            ResetSelf();
            
            
            _currentRotation++;

            
            _motionTween = rotatePivot.DORotate(new Vector3(0.0f, 90.0f, 0.0f), 0.125f, RotateMode.FastBeyond360).SetRelative(true).SetEase(Const.THIS.rotationEase);
            _motionTween.onUpdate = ResetSegmentRotations;
            
            _motionTween.onComplete += () =>
            {
                Busy = false;
            };
        }
        
        public void PunchRotate()
        {
            if (Busy)
            {
                return;
            }
            shakePivot.DOKill();
            shakePivot.localEulerAngles = Vector3.zero;

            ResetSelf();

            _motionTween = rotatePivot.DOPunchRotation(new Vector3(0.0f, 30.0f, 0.0f), 0.25f, 1);
            _motionTween.onUpdate = ResetSegmentRotations;
        }

        public void ResetSegmentRotations()
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
            
            rotatePivot.localEulerAngles = new Vector3(0.0f, _currentRotation * 90.0f, 0.0f);
            ResetSegmentRotations();

            _motionTween = transform.DOMove(position, duration).SetEase(ease).SetSpeedBased(speedBased).SetUpdate(true);
            _motionTween.onComplete += () =>
            {
                ResetSegmentRotations();
                Busy = false;
            };
        }

    }
}
