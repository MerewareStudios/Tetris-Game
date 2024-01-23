using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using Lofelt.NiceVibrations;
using UnityEngine;

namespace Game
{
    public class Map : Lazyingleton<Map>
    {
        [System.NonSerialized] public int MergeAudioIndex = 0;
        [System.NonSerialized] public static float TimeScale = 1.0f;
        
        
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
                Audio.Board_Merge_Riff.Play();
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


        public void ResetMergeAudioIndex()
        {
            MergeAudioIndex = 0;
        }

        public void Deconstruct()
        {
            ResetMergeAudioIndex();
        }
    }
}
