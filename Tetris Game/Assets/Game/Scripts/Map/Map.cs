using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Game
{
    public class Map : Lazyingleton<Map>
    {
        // [System.NonSerialized] private Coroutine _mainRoutine = null;
        // [System.NonSerialized] public bool MapWaitForCycle = false;
        [System.NonSerialized] public static int MergeAudioIndex = 0;
        [System.NonSerialized] public static float TimeScale = 1.0f;
        
        // public void StartMainLoop()
        // {
        //     StopLoop();
        //     _mainRoutine = StartCoroutine(MainLoop());
        //
        //     IEnumerator MainLoop()
        //     {
        //         yield return new WaitForSeconds(0.25f);
        //
        //         while (true)
        //         {
        //             // while (true)
        //             // {
        //                 // float waitOverride = Board.THIS.UsePowerups();
        //                 // if (!Board.THIS.HasDrop())
        //                 // {
        //                 //     break;
        //                 // }
        //                 // waitOverride = waitOverride >= 0.0f ? waitOverride : AnimConst.THIS.mergeTravelDelay + AnimConst.THIS.mergeTravelDur;
        //                 // yield return new WaitForSeconds(waitOverride);
        //             // }
        //             
        //             yield return new WaitForSeconds(1.5f);
        //             Board.THIS.CheckDeadLock();
        //             MapWaitForCycle = false;
        //         }
        //     }
        // }
        
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
                TimeScale = 0.0f;
                GameManager.UpdateTimeScale();
                Audio.Board_Merge_Riff.PlayOneShotPitch(1.0f, 0.95f + MergeAudioIndex * 0.05f);
                float duration = UIManager.THIS.comboText.Show(tetrisCount);


                yield return new WaitForSecondsRealtime(duration * 0.64f);
                TimeScale = 1.0f;
                GameManager.UpdateTimeScale();
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

        // private void StopLoop()
        // {
        //     if (_mainRoutine == null)
        //     {
        //         return;
        //     }
        //     StopCoroutine(_mainRoutine);
        //     _mainRoutine = null;
        // }

        public void Deconstruct()
        {
            ResetMergeAudioIndex();
            // StopLoop();
            // Map.THIS.MapWaitForCycle = false;
        }
        
        public void OnLevelEnd()
        {
            // StopLoop();
        }
    }
}
