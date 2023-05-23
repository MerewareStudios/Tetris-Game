using Internal.Core;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [SerializeField] private List<Place> places;
        [System.NonSerialized] private Dictionary<Transform, Place> placeDic = new();

        void Awake()
        {
            foreach (Place place in places)
            {
                placeDic.Add(place.transform, place);
            }
        }

        public Place GetPlace(Transform pt)
        {
            return placeDic[pt];
        }
    }
}
