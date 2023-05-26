using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        //[SerializeField] private List<Place> places;
        [SerializeField] private Grid grid;
        [System.NonSerialized] private List<Segment> segments = new();
        [System.NonSerialized] public int Tick = 0;

        IEnumerator Start()
        {
            grid.Construct();

            while (true)
            {
                grid.Tick();
                yield return new WaitForSeconds(GameManager.THIS.Constants.tickInterval);
                List<int> tetrisLines = grid.CheckTetris();
                grid.ClearLines(tetrisLines);
                yield return new WaitForSeconds(0.25f);
                if (tetrisLines.Count > 0)
                {
                    grid.MoveFromLine(tetrisLines[0]);
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
        public Place GetPlace(Transform pt)
        {
            return grid.placeDic[pt];
        }

        public void AddSegment(Segment segment)
        {
            segments.Add(segment);
        }

        public void Dehighlight()
        {
            grid.Dehighlight();
        }
    }
}
