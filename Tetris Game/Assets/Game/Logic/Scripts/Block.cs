using System;
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
        // Block has a general property of always moving forward with child blocks
        // If any of the child block is blocked by a forward pawn/obstacle it is placed and seperated 

        [SerializeField] private bool ghostBlock = false;
        [SerializeField] private List<Transform> segmentTransforms;
        [System.NonSerialized] public List<Pawn> pawns = new();
        [SerializeField] public Vector3 spawnerOffset;
        [System.NonSerialized] private Tween motionTween;
        [System.NonSerialized] private bool busy = false;

        public int PawnCount { get { return pawns.Count; } }

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
                //Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, Random.Range(1, 6));
                Pawn pawn = Spawner.THIS.SpawnPawn(this.transform, target.position, 1);
                pawn.MarkSpawnColor();
                pawn.parentBlock = this;
                pawns.Add(pawn);
            }
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
            if (!ghostBlock)
            {
                this.Despawn();
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
