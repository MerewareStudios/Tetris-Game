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
        [System.NonSerialized] private int _mergeAudioIndex = 0;
// impact metal, quick shing, horror positive level up, rising 1, open plain, 
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

                        Audio.Board_Merge_Cock.PlayOneShotPitch(1.0f, 0.8f + _mergeAudioIndex * 0.2f);

                        

                        if (tetrisLines.Count > 1)
                        {
                            Audio.Board_Merge_Combo.PlayOneShotPitch(1.0f, 0.6f + tetrisLines.Count * 0.2f);

                            float totalDuration = UIManager.THIS.comboText.Show(tetrisLines.Count);
                            yield return new WaitForSeconds(totalDuration * 0.5f);

                        }
                        else
                        {
                            Audio.Board_Merge_Single_Cheer.PlayOneShotPitch(1.0f, 0.8f + _mergeAudioIndex * 0.05f);

                            _mergeAudioIndex += 1;
                        }
                        Audio.Board_Pre_Merge.PlayOneShotPitch(1.0f, 1.0f + _mergeAudioIndex * 0.1f);

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

        public static void ResetMergeAudioIndex()
        {
            Map.THIS._mergeAudioIndex = 0;
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
            ResetMergeAudioIndex();
            StopLoop();
            Map.THIS.MapWaitForCycle = false;
        }
        
        public void OnLevelEnd()
        {
            StopLoop();
        }
    }
}
