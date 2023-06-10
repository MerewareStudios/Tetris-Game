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
        [System.NonSerialized] public Coroutine shootRoutine = null;
        [System.NonSerialized] public Coroutine mainRoutine = null;
        [System.NonSerialized] private bool canShoot = true;
        [System.NonSerialized] private float prevShoot = 0.0f;

        void Start()
        {
            grid.Construct();
        }

        public void Begin()
        {
            mainRoutine = StartCoroutine(MainLoop());
        }

        IEnumerator MainLoop()
        {
            shootRoutine = StartCoroutine(ShootRoutine());

            IEnumerator ShootRoutine()
            {
                while (true)
                {
                    if (Time.time - prevShoot > 1.5f)
                    {
                        if (canShoot)
                        {
                            grid.GiveBullet();
                            prevShoot = Time.time;
                        }
                    }

                    yield return null;
                }
            }
            
            while (true)
            {
                grid.Move(0.25f);
                yield return new WaitForSeconds(0.3f);
                grid.CheckSteady();

                List<int> tetrisLines = grid.CheckTetris();

                if (tetrisLines.Count > 0)
                {
                    if (tetrisLines.Count > 1)
                    {
                        UIManager.THIS.ft_Combo.FlyScreen("x" + tetrisLines.Count, Vector3.zero, 0.0f);
                        yield return new WaitForSeconds(0.4f);
                    }

                    canShoot = false;
                    
                    grid.MergeLines(tetrisLines, 0.2f);

                    grid.MarkNewMovers(tetrisLines[0]);
                    yield return new WaitForSeconds(0.75f);
                }
                
                yield return new WaitForSeconds(0.15f);
                canShoot = true;
            }
        }
        
        public void Deconstruct()
        {
            if (shootRoutine != null)
            {
                StopCoroutine(shootRoutine);
                shootRoutine = null;
            }
            if (mainRoutine != null)
            {
                StopCoroutine(mainRoutine);
                mainRoutine = null;
            }
            grid.Deconstruct();
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
                Pawn2Place(pawn).Accept(pawn, 0.1f);
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
            (Place place, bool canPlace) = Project(pawn);
            if (place == null)
            {
                return false;
            }
            return canPlace;
        }
       
        public void HighlightPawnOnGrid(Block block)
        {
            foreach (var pawn in block.pawns)
            {
                (Place place, bool canPlace) = Project(pawn);
                if (place == null)
                {
                    return;
                }
                if (canPlace)
                {
                    place.SetColor(Place.PlaceType.FREE);
                }
                else
                {
                    place.SetColor(Place.PlaceType.OCCUPIED);
                } 
            }
        }
        
        public (Place, bool) Project(Pawn pawn)
        {
            Vector2Int? index = Pos2Index(pawn.transform.position);
            if (index == null)
            {
                return (null, false);
            }
            Vector2Int ind = (Vector2Int)index;
            Place place = grid.GetPlace(ind);
            
            if (grid.size.y - pawn.parentBlock.Width > ind.y)
            {
                return (place, false);
            }
            if (place.Occupied)
            {
                return (place, false);
            }
            if (grid.HasForwardPawnAtColumn(ind))
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
