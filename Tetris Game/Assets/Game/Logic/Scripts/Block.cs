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
        [System.NonSerialized] public List<Pawn> pawns = new();
        [SerializeField] public List<Transform> pawnTargets;
        [System.NonSerialized] private Coroutine motionRoutine;
        [System.NonSerialized] private Vector3 targetPosition;
        [System.NonSerialized] private bool moving = false;
        [System.NonSerialized] private Direction queueDirection;
        [System.NonSerialized] private float forwardStamp;

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
                Pawn pawn = Pool.Pawn___Level_1.Spawn<Pawn>(this.transform);
                pawn.transform.position = target.position;
                pawn.transform.localScale = Vector3.one;
                pawn.transform.localRotation = Quaternion.identity;

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

        public void OnSpawn(Vector3 spawnPosition)
        {
            Construct();

            forwardStamp = Time.time - GameManager.THIS.Constants.forwardDelay;

            transform.position = spawnPosition;
            targetPosition = transform.position;
            motionRoutine = StartCoroutine(MotionRoutine());

            IEnumerator MotionRoutine()
            {
                while (true)
                {
                    while (moving)
                    {
                        yield return null;
                    }
                    if (Time.time - forwardStamp >= GameManager.THIS.Constants.forwardDelay)
                    {
                        //ReplenishMoveForward();
                        if (!CheckPlacement())
                        {
                            Move(Direction.FORWARD);
                        }
                    }
                    yield return null;
                }
            }
        }

        public void ReplenishMoveForward()
        {
            forwardStamp = Time.time;
        }

        private void Stop()
        {
            if (motionRoutine != null)
            {
                StopCoroutine(motionRoutine);
                motionRoutine = null;
            }
        }
        

        public void Move(Direction direction)
        {
            if (Map.THIS.grid.CheckMotion(Direction2Rule(direction), this) && !moving)
            {
                targetPosition += Direction2Position(direction);
                
                queueDirection = Direction.NONE;
                MoveToTargetPosition(GameManager.THIS.Constants.sidewayDuration, GameManager.THIS.Constants.sidewayEase, () =>
                    {
                        if (direction.Equals(Direction.FORWARD))
                        {
                            ReplenishMoveForward();
                        }
                        MovementEnd();
                    });
            }
        }

        public void QueueDirection(Direction direction)
        {
            if (!moving)
            {
                Move(direction);
                queueDirection = Direction.NONE;
                return;
            }
            queueDirection = direction;
        }

        private void MoveToTargetPosition(float duration, Ease ease, System.Action OnMoveComplete = null)
        {
            moving = true;
            transform.DOKill();
            transform.DOMove(targetPosition, duration).SetEase(ease)
               .onComplete += () =>
               {
                    moving = false;
                    OnMoveComplete.Invoke();
               };
        }

        private void MovementEnd()
        {
            if (Time.time - forwardStamp >= GameManager.THIS.Constants.forwardDelay)
            {
                queueDirection = Direction.NONE;
            }
            if (queueDirection.Equals(Direction.NONE))
            {
                return;
            }

            Move(queueDirection);
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
