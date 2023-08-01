using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;

[CreateAssetMenu(fileName = "Onboarding Data", menuName = "Game/Onboarding Data", order = 0)]
public class Onboarding : SSingleton<Onboarding>
{
    public float playerRotateDuration = 0.4f;
    public float needAmmoBubbleDelay = 1.5f;
    public float firstBlockSpawnDelay = 1.5f;
    public float firstBlockBubbleDelay = 0.5f;
    public float niceOneSkipDelay = 0.5f;
    public float beginDelay = 0.5f;
    
    
    public static void AmmoPlacementCheck()
    {
        if (ONBOARDING.NEED_AMMO_TEXT_SHOWN.IsNotComplete())
        {
            Warzone.THIS.Player.RotateToPlayer(Onboarding.THIS.playerRotateDuration);
            UIManager.THIS.speechBubble.Speak("I need ammo!", Onboarding.THIS.needAmmoBubbleDelay);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);

            DOVirtual.DelayedCall(Onboarding.THIS.firstBlockSpawnDelay, () =>
            {
                DOVirtual.DelayedCall(0.35f, () =>
                {
                    UIManager.THIS.speechBubble.Speak("Use that ammo box!", Onboarding.THIS.firstBlockBubbleDelay);
                    Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);
                
                    ONBOARDING.NEED_AMMO_TEXT_SHOWN.SetComplete();
                });

                UIManager.THIS.speechBubble.Hide();

                Spawner.THIS.Begin(0.0f);
            });
        }
    }
    
    public static void ShowAmmoBoxMerge()
    {
        Warzone.THIS.Player.RotateToPlayer(Onboarding.THIS.playerRotateDuration);
        UIManager.THIS.speechBubble.Speak("Stack those boxes here!", Onboarding.THIS.needAmmoBubbleDelay);
        Warzone.THIS.Player.animator.SetTrigger(Player.SHOW_HASH);
        
        ONBOARDING.TEACH_MERGE_PLACE.SetComplete();
    }
    
    public static void ApriciateForMerge()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.Replenish();
            UIManager.THIS.speechBubble.Speak("Nice one!", 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);
            ONBOARDING.HAVE_MERGED.SetComplete();

            yield return new WaitForSeconds(1.5f);
            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(0.2f);
            UIManager.THIS.speechBubble.Speak("Keep stacking!", 0.2f);
            
            yield return new WaitForSeconds(2.0f);

            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(1.0f);
            Warzone.THIS.Player.animator.SetTrigger(Player.IDLE_HASH);
            
            
            // yield return new WaitForSeconds(0.25f);

            Warzone.THIS.Begin();
        }
    }
}
