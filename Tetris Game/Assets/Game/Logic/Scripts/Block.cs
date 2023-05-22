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
        [SerializeField] public Vector3 offset;
        [System.NonSerialized] public List<Pawn> pawns = new();
        [SerializeField] public List<Transform> pawnTargets;
        [SerializeField] public Transform rotatePivot;
        [System.NonSerialized] private Coroutine motionRoutine;
        [System.NonSerialized] private Vector3 targetPosition;
        [System.NonSerialized] private Direction queueDirection;
        [System.NonSerialized] private float moveTimeStamp;

        private bool Moving { get; set; }
        private bool Rotating { get; set; }

        private void OnDrawGizmos()
        {
            foreach (var target in pawnTargets)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(target.position, Vector3.one * 0.95f);
            }
        }

        private void Construct()
        {
            foreach (var target in pawnTargets)
            {
                Pawn pawn = Pool.Pawn___Level_1.Spawn<Pawn>(this.rotatePivot);
                pawn.transform.position = target.position;
                pawn.transform.localScale = Vector3.one;
                pawn.transform.localRotation = Quaternion.identity;
                pawn.Res();
                pawns.Add(pawn);
            }
        }
        public void Deconstruct(Transform pawnParent)
        {
            foreach (var pawn in pawns)
            {
                pawn.transform.parent = pawnParent;
            }
            pawns.Clear();
            this.Despawn();
        }

        public void OnSpawn()
        {
            Construct();

            moveTimeStamp = Time.time - GameManager.THIS.Constants.forwardDelay;

            targetPosition = transform.position;
            motionRoutine = StartCoroutine(MotionRoutine());

            IEnumerator MotionRoutine()
            {
                while (true)
                {
                    while (Moving || Rotating)
                    {
                        yield return null;
                    }
                    if (Time.time - moveTimeStamp >= GameManager.THIS.Constants.forwardDelay)
                    {
                        if (!CheckPlacement())
                        {
                            Move(Direction.FORWARD, false, GameManager.THIS.Constants.forwardMovementDuration);
                        }
                    }
                    yield return null;
                }
            }
        }

        public void StampTime()
        {
            moveTimeStamp = Time.time;
        }
        public void ByPassTime(float drag)
        {
            moveTimeStamp = Time.time - GameManager.THIS.Constants.forwardDelay + drag;
        }

        private void Stop()
        {
            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
                motionRoutine = null;
            }
        }
        

        public void Move(Direction direction, bool useAfterMovementDelay, float duration)
        {
            if (Map.THIS.grid.CheckMotion(Direction2Rule(direction), this) && !Moving)
            {
                targetPosition += Direction2Position(direction);
                
                queueDirection = Direction.NONE;
                MoveToTargetPosition(duration, Ease.Linear, () =>
                    {
                        if (direction.Equals(Direction.FORWARD))
                        {
                            StampTime();
                        }
                        MovementEnd(useAfterMovementDelay);
                    });
            }
        }

        public void QueueDirection(Direction direction)
        {
            if (!Moving)
            {
                Move(direction, true, GameManager.THIS.Constants.sideMovementDuration);
                queueDirection = Direction.NONE;
                return;
            }
            queueDirection = direction;
        }

        public void Rotate()
        {
            if (Moving)
            {
                return;
            }
            if (Rotating)
            {
                return;
            }
            Rotating = true;
            this.rotatePivot.DOKill();
            this.rotatePivot.DORotate(new Vector3(0.0f, 90.0f, 0.0f), 0.25f, RotateMode.WorldAxisAdd)
                .SetRelative(true)
                .SetEase(Ease.OutSine)
                .onComplete += 
                    () => 
                    {
                        Rotating = false;
                        ByPassTime(0.3f);
                    };
        }

        private void MoveToTargetPosition(float duration, Ease ease, System.Action OnMoveComplete = null)
        {
            Moving = true;
            transform.DOKill();
            transform.DOMove(targetPosition, duration).SetEase(ease)
               .onComplete += () =>
               {
                    Moving = false;
                    OnMoveComplete.Invoke();
               };
        }

        private void MovementEnd(bool useAfterMovementDelay)
        {
            float delay = useAfterMovementDelay ? GameManager.THIS.Constants.afterMovementDelay : GameManager.THIS.Constants.forwardDelay;
            ByPassTime(delay);

            if (queueDirection.Equals(Direction.NONE))
            {
                return;
            }

            Move(queueDirection, true, GameManager.THIS.Constants.sideMovementDuration);
        }
        private bool CheckPlacement()
        {
            if (!Map.THIS.grid.CheckMotion(Map.THIS.grid.CanMoveForward, this))
            {
                Stop();
                Map.THIS.BlockPlaced();
                return true;
            }
            return false;
        }
        public void Mark()
        {
            foreach (var pawn in pawns)
            {
                pawn.Mark();
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
        public enum Direction
        {
            NONE,
            FORWARD,
            BACKWARD,
            LEFT,
            RIGHT
        }

        private Vector3 Direction2Position(Direction direction)
        {
            switch (direction)
            {
                case Direction.FORWARD:
                    return Vector3.forward;
                case Direction.BACKWARD:
                    return Vector3.back;
                case Direction.LEFT:
                    return Vector3.left;
                case Direction.RIGHT:
                    return Vector3.right;

            }
            return Vector3.zero;
        }
        private Grid.CheckIndexFunction Direction2Rule(Direction direction)
        {
            switch (direction)
            {
                case Direction.FORWARD:
                    return Map.THIS.grid.CanMoveForward;
                case Direction.BACKWARD:
                    return null;
                case Direction.LEFT:
                    return Map.THIS.grid.CanMoveLeft;
                case Direction.RIGHT:
                    return Map.THIS.grid.CanMoveRight;

            }
            return null;
        }
    }
}
