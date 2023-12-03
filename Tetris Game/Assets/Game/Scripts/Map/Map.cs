using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Map : Singleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
        [System.NonSerialized] public bool MapWaitForCycle = false;
        // [System.NonSerialized] public static bool Moving = false;

        public void StartMainLoop()
        {
            StopLoop();
            _mainRoutine = StartCoroutine(MainLoop());

            // Moving = false;

            IEnumerator MainLoop()
            {
                yield return new WaitForSeconds(0.25f);

                while (true)
                {
                    Board.THIS.CheckAll();

                    // Map.Moving = true;
                    Board.THIS.MoveAll(0.2f); // 1
                    Board.THIS.HighlightPlaces();

                    yield return new WaitForSeconds(0.2f);
                    yield return new WaitForEndOfFrame();
                    
                    
                    Board.THIS.CheckAll();
                    Board.THIS.HighlightPlaces();
                    


                    while (true)
                    {
                        float waitOverride = Board.THIS.UsePowerups();
                        if (!Board.THIS.HasDrop())
                        {
                            break;
                        }

                        Board.THIS.MarkDropPointsMover();
                        Board.THIS.CheckAll();
                        Board.THIS.HighlightPlaces();

                        waitOverride = waitOverride >= 0.0f ? waitOverride : AnimConst.THIS.mergeTravelDelay + AnimConst.THIS.mergeTravelDur;
                        yield return new WaitForSeconds(waitOverride);
                    }

                    
                    List<int> tetrisLines = Board.THIS.CheckTetris();
                    
                    if (tetrisLines.Count > 0)
                    {
                        if (tetrisLines.Count > 1)
                        {
                            float totalDuration = UIManager.THIS.comboText.Show(tetrisLines.Count);
                            yield return new WaitForSeconds(totalDuration * 0.5f);

                        }
                        Board.THIS.MergeLines(tetrisLines);
                    
                        Board.THIS.MarkMoverByTetris(tetrisLines);
                        Board.THIS.CheckAll();
                        Board.THIS.HighlightPlaces();

                        yield return new WaitForSeconds(AnimConst.THIS.mergeTravelDelay + AnimConst.THIS.mergeTravelDur);
                    }
                    yield return new WaitForSeconds(0.2f);
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
