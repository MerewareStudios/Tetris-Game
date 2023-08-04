using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;

[CreateAssetMenu(fileName = "Onboarding Data", menuName = "Game/Onboarding Data", order = 0)]
public class Onboarding : SSingleton<Onboarding>
{
    [TextArea] [SerializeField] public string needAmmoText;
    [TextArea] [SerializeField] public string useAmmoBoxText;
    [TextArea] [SerializeField] public string stackText;
    [TextArea] [SerializeField] public string niceOneText;
    [TextArea] [SerializeField] public string keepStackingText;
    [TextArea] [SerializeField] public string enemiesComingText;
    
    
    public static void AmmoPlacementCheck()
    {
        if (ONBOARDING.NEED_AMMO_TEXT_SHOWN.IsNotComplete())
        {
            Warzone.THIS.Player.RotateToPlayer(0.5f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needAmmoText, 0.5f);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);

            DOVirtual.DelayedCall(2.75f, () =>
            {
                DOVirtual.DelayedCall(0.35f, () =>
                {
                    UIManager.THIS.speechBubble.Speak(Onboarding.THIS.useAmmoBoxText, 0.4f);
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
        Warzone.THIS.Player.RotateToPlayer(0.5f);
        UIManager.THIS.speechBubble.Speak(Onboarding.THIS.stackText, 0.5f);
        Warzone.THIS.Player.animator.SetTrigger(Player.SHOW_HASH);
        
        ONBOARDING.TEACH_MERGE_PLACE.SetComplete();
    }
    
    public static void ApriciateForMerge()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.Replenish();
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.niceOneText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);
            ONBOARDING.HAVE_MERGED.SetComplete();

            yield return new WaitForSeconds(1.75f);
            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(0.1f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.keepStackingText, 0.2f);
            
            yield return new WaitForSeconds(3.0f);

            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(1.0f);
            Warzone.THIS.Player.animator.SetTrigger(Player.IDLE_HASH);
            
            
            // yield return new WaitForSeconds(0.25f);

            Warzone.THIS.Begin();
        }
    }
}
