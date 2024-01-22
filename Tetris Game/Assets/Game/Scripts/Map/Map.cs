using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Game
{
    public class Map : Lazyingleton<Map>
    {
        [System.NonSerialized] private Coroutine _mainRoutine = null;
        [System.NonSerialized] public bool MapWaitForCycle = false;
        [System.NonSerialized] public static int MergeAudioIndex = 0;
        
        public void StartMainLoop()
        {
            StopLoop();
            _mainRoutine = StartCoroutine(MainLoop());

            IEnumerator MainLoop()
            {
                yield return new WaitForSeconds(0.25f);

                while (true)
                {
                    while (true)
                    {
                        float waitOverride = Board.THIS.UsePowerups();
                        if (!Board.THIS.HasDrop())
                        {
                            break;
                        }
                        waitOverride = waitOverride >= 0.0f ? waitOverride : AnimConst.THIS.mergeTravelDelay + AnimConst.THIS.mergeTravelDur;
                        yield return new WaitForSeconds(waitOverride);
                    }
                    
                    yield return new WaitForSeconds(0.2f);
                    Board.THIS.CheckDeadLock();
                    MapWaitForCycle = false;
                }
            }
        }
        
        public void CheckTetris(List<Place> places)
        {
            int tetrisCount = Board.THIS.CheckTetris(places);

            if (tetrisCount == 0)
            {
                return;
            }
            
            StartCoroutine(Calls(tetrisCount));
        }
        
        IEnumerator Calls(int tetrisCount)
        {
            if (tetrisCount > 1)
            {
                Time.timeScale = 0.0f;
                Audio.Board_Merge_Riff.PlayOneShotPitch(1.0f, 0.95f + MergeAudioIndex * 0.05f);
                float duration = UIManager.THIS.comboText.Show(tetrisCount);


                yield return new WaitForSecondsRealtime(duration);
                Time.timeScale = 1.0f;
            }
                        
            MergeAudioIndex += tetrisCount;
                
            Audio.Board_Merge_Cock.PlayOneShotPitch(1.0f, 1.0f);
            Audio.Board_Merge_Rising.PlayOneShotPitch(1.0f, 0.65f + MergeAudioIndex * 0.05f);
            Audio.Board_Pre_Merge.PlayOneShotPitch(0.75f, 1.0f);
                
            HapticManager.Vibrate(HapticPatterns.PresetType.MediumImpact);
        }


        public static void ResetMergeAudioIndex()
        {
            MergeAudioIndex = 0;
        }

        private void StopLoop()
        {
            if (_mainRoutine == null)
            {
                return;
            }
            StopCoroutine(_mainRoutine);
            _mainRoutine = null;
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
