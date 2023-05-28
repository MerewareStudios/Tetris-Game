using Internal.Core;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Segment : MonoBehaviour
    {
        [System.NonSerialized] public Block parentBlock;
        [System.NonSerialized] public Place currentPlace;
        [System.NonSerialized] public Place forwardPlace;
        [System.NonSerialized] private bool highlightRegardless = false;
        [SerializeField] public MeshRenderer meshRenderer;
        [SerializeField] public int level = 1;
        [SerializeField] public TextMeshPro levelText;
        [SerializeField] public int tick;
        [SerializeField] public bool Mover = false;
        [SerializeField] private int freeForward = 0;

        public void SetDeckColor()
        {
            meshRenderer.SetColor(GameManager.MPB_SEGMENT, "_BaseColor", GameManager.THIS.Constants.segmentDeck);
        }
        public void SetMergeColor()
        {
            meshRenderer.SetColor(GameManager.MPB_SEGMENT, "_BaseColor", GameManager.THIS.Constants.segmentMerge);
        }
        public void SetFreeMoverColor()
        {
            meshRenderer.SetColor(GameManager.MPB_SEGMENT, "_BaseColor", GameManager.THIS.Constants.segmentFreeMover);
        }
        public void SetMoverColor()
        {
            meshRenderer.SetColor(GameManager.MPB_SEGMENT, "_BaseColor", GameManager.THIS.Constants.segmentMover);
        }
        public void SetSteadyColor()
        {
            meshRenderer.SetColor(GameManager.MPB_SEGMENT, "_BaseColor", GameManager.THIS.Constants.segmentSteady);
        }

        public bool FreeMover
        {
            get
            {
                return freeForward > 0;
            }
        }
        public int FreeForward
        {
            set
            {
                freeForward = value;
                //Color color = (value > 0) ? GameManager.THIS.Constants.segmentFreeMover : GameManager.THIS.Constants.segmentSteady;
                //meshRenderer.SetColor(GameManager.MPB_SEGMENT, "_BaseColor", color);
            }
            get
            {
                return freeForward;
            }
        }


        public void SetLevel(int level)
        {
            this.level = level;
            levelText.text = level.ToString();
        }

        public void Disjoint()
        {
            if (parentBlock != null)
            {
                parentBlock.Deconstruct();
            }
            currentPlace = null;
            forwardPlace = null;
            Mover = false;
            FreeForward = 0;
            //this.Despawn();
        }
        public bool CanSubmit
        {
            get 
            {
                return this.currentPlace != null && !this.currentPlace.Occupied;    
            }
        }
        public void AddFreeMove(int count)
        {
            SetFreeMoverColor();
            FreeForward += count;
        }
        public Place Check()
        {
            currentPlace = null;
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10.0f, GameManager.THIS.Constants.segmentPlaceLayer))
            {
                currentPlace = Map.THIS.GetPlace(hit.transform);
                if (currentPlace.Occupied)
                {
                    currentPlace.Deny = true;
                }
                else
                {
                    currentPlace.Highlight = true;
                }
                return currentPlace;
            }
            return currentPlace;
        }

        private Place CheckPlace(Vector3 position)
        {
            if (Physics.Raycast(position, Vector3.down, out RaycastHit hit, 10.0f, GameManager.THIS.Constants.segmentPlaceLayer))
            {
                return Map.THIS.GetPlace(hit.transform);
            }
            return null;
        }
        public void Submit()
        {
            SetMoverColor();
            this.currentPlace.Accept(this);
            this.Mover = true;
        }
        public void UpdateParentBlockStats(int tick)
        {
            if (FreeForward <= 0 && !Mover)
            {
                return;
            }
            this.tick = tick;

            forwardPlace = CheckPlace(transform.position + Vector3.forward);

            bool state = (forwardPlace == null) || (forwardPlace.currentSegment != null && !forwardPlace.currentSegment.Mover && !forwardPlace.currentSegment.FreeMover);
            if (state)
            {
                if (parentBlock != null)
                {
                    parentBlock.Deconstruct();
                }
                Mover = false;
                if (FreeMover)
                {
                    highlightRegardless = true;
                }
                FreeForward = 0;
            }
        }
        public void MoveForward()
        {
            if (!Mover && !FreeMover)
            {
                if (highlightRegardless)
                {
                    SetFreeMoverColor();
                    highlightRegardless = false;
                    return;
                }
                SetSteadyColor();
                return;
            }

            if (FreeMover)
            {
                SetFreeMoverColor();
                Mover = false;
                FreeForward--;
            }
            else
            {
                SetMoverColor();
            }
            if (this.currentPlace != null)
            {
                this.currentPlace.Clear();
            }
            this.currentPlace = forwardPlace;
            this.currentPlace.Accept(this);
        }
    }
}
