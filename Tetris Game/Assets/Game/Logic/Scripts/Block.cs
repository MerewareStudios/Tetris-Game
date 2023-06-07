using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Block : MonoBehaviour
    {
        // Block has a general property of always moving forward with child blocks
        // If any of the child block is blocked by a forward pawn/obstacle it is placed and seperated 

        [SerializeField] private int[] Widths;
        [SerializeField] private List<Transform> segmentTransforms;
        [System.NonSerialized] public List<Pawn> pawns = new();
        [SerializeField] public Vector3 spawnerOffset;
        [System.NonSerialized] private Tween motionTween;
        [System.NonSerialized] private bool busy = false;

        public int PawnCount { get { return pawns.Count; } }

        public int Width
        {
            get
            {
                int rotIndex = Mathf.FloorToInt(transform.eulerAngles.y / 90.0f);
                rotIndex %= 2;
                return Mathf.Clamp(Widths[rotIndex], 1, int.MaxValue);
                // return Widths[rotIndex] + 1;
            }
        }
        public int NextWidth
        {
            get
            {
                int rotIndex = Mathf.FloorToInt((transform.eulerAngles.y + 90.0f) / 90.0f);
                rotIndex %= 2;
                return Mathf.Clamp(Widths[rotIndex], 1, int.MaxValue);
                // return Widths[rotIndex] + 1;
            }
        }

        private void OnDrawGizmos()
        {
            foreach (var segmentTransform in segmentTransforms)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(segmentTransform.position, Vector3.one * 1f);
            }
        }

        public void Construct()
        {
            foreach (var target in segmentTransforms)
            {
                // Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, Random.Range(1, 6));
                Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, 1);
                pawn.MarkSpawnColor();
                pawn.parentBlock = this;
                pawn.Show();
                pawns.Add(pawn);
            }
        }
        public void Deconstruct()
        {
            foreach (var pawn in pawns)
            {
                pawn.Deconstruct();
            }
            motionTween?.Kill();
            busy = false;
            pawns.Clear();
            this.Despawn();
        }
        public void Add(Pawn pawn)
        {
            pawns.Add(pawn);
            pawn.parentBlock = this;
        }
        public void Detach()
        {
            foreach (var pawn in pawns)
            {
                pawn.parentBlock = null;
            }
            pawns.Clear();
            this.Despawn();
        }

        public void Rotate(System.Action OnComplete = null)
        {
            if (busy)
            {
                return;
            }
            busy = true;

            motionTween?.Kill();
            motionTween = transform.DORotate(new Vector3(0.0f, 90.0f, 0.0f), GameManager.THIS.Constants.rotationDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(GameManager.THIS.Constants.rotationEase);
            motionTween.onUpdate += () => 
                {
                    foreach (var segment in pawns)
                    {
                        segment.transform.rotation = Quaternion.identity;
                    }
                };
            motionTween.onComplete += () =>
            {
                busy = false;
                OnComplete?.Invoke();   
            };
        }
        public void Move(Vector3 position, float duration, Ease ease, bool speedBased = false)
        {
            if (busy)
            {
                return;
            }
            busy = true;

            motionTween?.Kill();
            motionTween = transform.DOMove(position, duration).SetEase(ease).SetSpeedBased(speedBased);
            motionTween.onComplete += () =>
            {
                busy = false;
            };
        }

    }
}
