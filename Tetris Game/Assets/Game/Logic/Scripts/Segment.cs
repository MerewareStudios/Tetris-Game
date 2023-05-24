using UnityEngine;

namespace Game
{
    public class Segment : MonoBehaviour
    {
        [System.NonSerialized] public Block parentBlock;
        [System.NonSerialized] public Place currentPlace;
        [System.NonSerialized] public Place forwardPlace;
        [SerializeField] public bool Mover = false;

        public bool CanSubmit
        {
            get 
            {
                return this.currentPlace != null && !this.currentPlace.Occupied;    
            }
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
            this.currentPlace.Accept(this);
            this.Mover = true;
        }
        public void UpdateParentBlockStats()
        {
            if (!Mover || parentBlock == null)
            {
                return;
            }

            forwardPlace = CheckPlace(transform.position + Vector3.forward);

            bool state = (forwardPlace == null) || (forwardPlace.currentSegment != null && !forwardPlace.currentSegment.Mover);
            if (state)
            {
                parentBlock.Deconstruct();
            }
        }
        public void MoveForward()
        {
            if (!Mover)
            {
                return;
            }
            if (parentBlock == null)
            {
                return;
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
