using DG.Tweening;
using Internal.Core;
using RootMotion.Demos;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public class Pawn : MonoBehaviour
    {
        [SerializeField] public MeshRenderer meshRenderer;
        [SerializeField] public TextMeshPro levelText;

        [System.NonSerialized] public Block parentBlock;
        [System.NonSerialized] public int level = 1;
        [System.NonSerialized] public int movedAtTick = -1;
        [System.NonSerialized] public bool Mover = false;
        [System.NonSerialized] public bool MoveUntilForward = false;
        public bool Connected { get { return parentBlock != null; } }

        public void Construct(int level)
        {
            this.level = level;
            levelText.text = level.ToString();
            movedAtTick = -1;
        }

        public void Move(Vector3 position, float duration, Ease ease, System.Action OnComplete = null)
        {
            transform.DOKill();
            transform.DOMove(position, duration).SetEase(ease)
                .onComplete += () =>
                {
                    OnComplete?.Invoke();
                };
        }
        public void Move(Transform parent, Vector3 position)
        {
            transform.parent = parent;
            transform.position = position;
        }

        #region Colors
        public void MarkSpawnColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.spawnColor);
        }
        public void MarkMoverColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.moverColor);
        }
        public void MarkSteadyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_PAWN, "_BaseColor", GameManager.THIS.Constants.steadyColor);
        }
        #endregion


        public void MoveForward(Place checkerPlace, int tick, float moveDuration)
        {
            (Place forwardPlace, bool shouldStay) = ShouldStay(checkerPlace);
            if (shouldStay)
            {
                Mover = false;
                return;
            }
            this.movedAtTick = tick;
            Mover = true;
            forwardPlace.Accept(this, moveDuration, () =>
            {
                checkerPlace.Current = null;
            });
        }
        public void CheckSteady(Place checkerPlace)
        {
            (Place forwardPlace, bool shouldStay) = ShouldStay(checkerPlace);

            if (shouldStay)
            {
                if (MoveUntilForward)
                {
                    Map.THIS.grid.SetFrontFree(checkerPlace.index.x, false);
                }
                MoveUntilForward = false;
                MarkSteadyColor();
                if (Connected)
                {
                    parentBlock.Detach();
                }
            }
        }
        public (Place, bool) ShouldStay(Place checkerPlace)
        {
            Place forwardPlace = Map.THIS.GetForwardPlace(checkerPlace);

            if (forwardPlace == null)
            {
                return (null, true);
            }
            if (forwardPlace.Occupied && !forwardPlace.Current.Connected && !forwardPlace.Current.Mover)
            {
                return (forwardPlace, true);
            }
            if (MoveUntilForward && Map.THIS.grid.IsFrontFree(checkerPlace.index.x))
            {
                //if ()
                //{
                    //return (forwardPlace, true);
                //}
                return (forwardPlace, false);
            }
            if (!Connected)
            {
                return (forwardPlace, true);
            }

            return (forwardPlace, false);
        }
    }
}
