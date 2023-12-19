using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Game
{
    public class Map : Lazyingleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
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
                        HapticManager.Vibrate(HapticPatterns.PresetType.MediumImpact);

                        Audio.Pre_Merge_Cheer.PlayOneShot();

                        if (tetrisLines.Count > 1)
                        {
                            
                            float totalDuration = UIManager.THIS.comboText.Show(tetrisLines.Count);
                            yield return new WaitForSeconds(totalDuration * 0.5f);

                        }
                        Audio.Pre_Merge.PlayOneShot();

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
