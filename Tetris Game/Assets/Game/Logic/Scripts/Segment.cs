using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game
{
    public class Segment : MonoBehaviour
    {
        [System.NonSerialized] public Place currentPlace;
        public Place Check()
        {
            if (currentPlace != null)
            {
                currentPlace.Highlight = false;
                currentPlace = null;
            }
            
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 10.0f, GameManager.THIS.Constants.segmentPlaceLayer))
            {
                Debug.Log("check");

                currentPlace = Map.THIS.GetPlace(hit.transform);
                currentPlace.Highlight = true;
                return currentPlace;
            }
            return currentPlace;
        }

    }
}
