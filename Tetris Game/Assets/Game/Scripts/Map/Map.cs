using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
        // [System.NonSerialized] private Queue<int> _queuedMergeLines = new();
        [System.NonSerialized] public bool MapWaitForCycle = false;

        public void StartMainLoop()
        {
            StopLoop();
            _mainRoutine = StartCoroutine(MainLoop());

            IEnumerator MainLoop()
            {
                yield return new WaitForSeconds(0.25f);

                while (true)
                {
                    Board.THIS.CheckAll();

                    Board.THIS.MoveAll(0.2f);
                    Spawner.THIS.HighlightCurrentBlock();

                    // yield return new WaitWhile(() => moving);
                    
                    yield return new WaitForSeconds(0.25f);
                    Board.THIS.CheckAll();
                    Spawner.THIS.HighlightCurrentBlock();
                    


                    while (true)
                    {
                        List<Vector2Int> moverPoints = Board.THIS.UsePowerups();
                        if (moverPoints.Count == 0)
                        {
                            break;
                        }

                        Board.THIS.MarkMover(moverPoints);
                        Board.THIS.CheckAll();
                        Spawner.THIS.HighlightCurrentBlock();
                        yield return new WaitForSeconds(AnimConst.THIS.mergeTravelDelay + AnimConst.THIS.mergeTravelDur);
                    }

                    
                    List<int> tetrisLines = Board.THIS.CheckTetris();
                    
                    if (tetrisLines.Count > 0)
                    {
                        if (tetrisLines.Count > 1)
                        {
                            UIManager.THIS.ShowCombo(tetrisLines.Count);
                            Warzone.THIS.Player.Gun.Boost(tetrisLines.Count);
                        }
                        Board.THIS.MergeLines(tetrisLines);
                    
                        Board.THIS.MarkMover(tetrisLines[0]);
                        Board.THIS.CheckAll();
                        Spawner.THIS.HighlightCurrentBlock();

                        yield return new WaitForSeconds(AnimConst.THIS.mergeTravelDelay + AnimConst.THIS.mergeTravelDur);
                    }
                    yield return new WaitForSeconds(0.25f);
                    Board.THIS.CheckDeadLock();
                    MapWaitForCycle = false;
                }
            }
        }

        private void StopLoop()
        {
            if (_mainRoutine != null)
            {
                StopCoroutine(_mainRoutine);
                _mainRoutine = null;
            }
        }

        // public void ForceMerge(int line)
        // {
        //     _queuedMergeLines.Enqueue(line);
        // }

        public void Deconstruct()
        {
            StopLoop();
            Map.THIS.MapWaitForCycle = false;
        }
        
        public void OnLevelEnd()
        {
            StopLoop();
        }
    }
}
