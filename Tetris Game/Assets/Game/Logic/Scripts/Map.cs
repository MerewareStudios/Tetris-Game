using DG.Tweening;
using Internal.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [SerializeField] public Grid grid;
        [SerializeField] private Vector3 indexOffset;
        [System.NonSerialized] private List<Pawn> segments = new();
        [System.NonSerialized] public int Tick = 0;
        [System.NonSerialized] public int FreeMoveIndex = 99;

        IEnumerator Start()
        {
            grid.Construct();

            while (true)
            {
                grid.Move(0.25f);
                FreeMoveIndex = 99;
                yield return new WaitForSeconds(0.35f);
                grid.CheckSteady();

                List<int> tetrisLines = grid.CheckTetris();
                grid.MergeLines(tetrisLines, 0.2f);

                if (tetrisLines.Count > 0)
                {
                    grid.MarkNewMovers(tetrisLines[0], tetrisLines.Count);
                    yield return new WaitForSeconds(0.35f);
                }

                yield return new WaitForSeconds(0.05f);
                
            }
        }
        public Place GetPlace(Transform pt)
        {
            return grid.placeDic[pt];
        }

        public void AddSegment(Pawn segment)
        {
            segments.Add(segment);
        }

        public void Dehighlight()
        {
            grid.Dehighlight();
        }
        public void PlaceBlockOnGrid(Block block)
        {
            foreach (var pawn in block.pawns)
            {
                pawn.MarkMoverColor();
                Pawn2Place(pawn).Accept(pawn, 0.2f);
            }
        }
        public bool CanPlaceBlockOnGrid(Block block)
        {
            foreach (var pawn in block.pawns)
            {
                if (!CanPlacePawnOnGrid(pawn))
                {
                    return false;
                }
            }
            return true;
        }
        public bool CanPlacePawnOnGrid(Pawn pawn)
        {
            Vector2Int? index = Pos2Index(pawn.transform.position);
            if (index == null)
            {
                return false;
            }
            Place place = grid.GetPlace((Vector2Int)index);
            if (place.Occupied)
            {
                return false;
            }
            return true;
        }
       
        public void HighlightPawnOnGrid(Block block)
        {
            bool canPlaceAll = true;
            List<Place> places = new();
            foreach (var pawn in block.pawns)
            {
                (Place place, bool canPlace) = Project(pawn);
                if (place != null)
                {
                    places.Add(place);
                }
                if (!canPlace)
                {
                    canPlaceAll = false;
                }
            }
            foreach (var place in places)
            {
                if (canPlaceAll) place.MarkFree(); else place.MarkOccupied(); 
            }
        }
        public (Place, bool) Project(Pawn pawn)
        {
            Vector2Int? index = Pos2Index(pawn.transform.position);
            if (index == null)
            {
                return (null, false);
            }
            Place place = grid.GetPlace((Vector2Int)index);
            if (place.Occupied)
            {
                return (place, false);
            }
            return (place, true);
        }

        public Place Pawn2Place(Pawn pawn)
        {
            Vector2Int? index = Pos2Index(pawn.transform.position);
            if (index != null)
            {
                return grid.GetPlace((Vector2Int)index);
            }
            return null;
        }

        public Vector2Int? Pos2Index(Vector3 position)
        {
            Vector3 posDif = position - transform.position + indexOffset;
            Vector2 posFin = new Vector2(posDif.x, -posDif.z);
            Vector2Int? index = null;
            if (posFin.x >= 0.0f && posFin.x < grid.size.x && posFin.y >= 0.0f && posFin.y < grid.size.y)
            {
                index = new Vector2Int((int)posFin.x, (int)posFin.y);
            }
            return index;
        }

        public Place GetForwardPlace(Place place)
        {
            Place forwardPlace = null;

            Vector2Int index = place.index;
            index.y--;
            if (index.y >= 0)
            {
                forwardPlace = grid.GetPlace(index);
            }
            return forwardPlace;
        }
    }
}
