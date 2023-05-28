using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [SerializeField] private Grid grid;
        [System.NonSerialized] private List<Segment> segments = new();
        [System.NonSerialized] public int Tick = 0;
        [System.NonSerialized] public int FreeForward = 0;

        public bool HasFreeForward
        {
            get
            {
                return FreeForward > 0;
            }
        }

        IEnumerator Start()
        {
            grid.Construct();

            while (true)
            {
                grid.Tick();
                List<int> tetrisLines = grid.CheckTetris();
                grid.MergeLines(tetrisLines);
                if (tetrisLines.Count > 0)
                {
                    yield return new WaitForSeconds(1.0f);
                }
                if (tetrisLines.Count > 0)
                {
                    yield return new WaitForSeconds(0.15f);
                    grid.MoveFromLine(tetrisLines[0], tetrisLines.Count);
                    yield return new WaitForSeconds(0.75f);

                }
                yield return new WaitForSeconds(GameManager.THIS.Constants.tickInterval);
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
