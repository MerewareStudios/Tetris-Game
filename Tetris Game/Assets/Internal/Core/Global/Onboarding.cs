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
    
    public static void AmmoPlacementCheck()
    {
        if (ONBOARDING.NEED_AMMO_TEXT_SHOWN.IsNotComplete())
        {
            Warzone.THIS.Player.RotateToPlayer(Onboarding.THIS.playerRotateDuration);
            UIManager.THIS.speechBubble.Speak("I need ammo!", Onboarding.THIS.needAmmoBubbleDelay);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);

            ShowAmmoBoxUse();
        }
    }
    
    public static void ShowAmmoBoxUse()
    {
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

    public static void ShowAmmoBoxMerge()
    {
        if (ONBOARDING.TEACH_MERGE_PLACE.IsNotComplete())
        {
            // UIManager.THIS.speechBubble.Hide();

            Warzone.THIS.Player.RotateToPlayer(Onboarding.THIS.playerRotateDuration);
            UIManager.THIS.speechBubble.Speak("Merge those boxes here!", Onboarding.THIS.needAmmoBubbleDelay);
            Warzone.THIS.Player.animator.SetTrigger(Player.SHOW_HASH);
        }
    }
}