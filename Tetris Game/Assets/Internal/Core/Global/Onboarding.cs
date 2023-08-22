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
    [TextArea] [SerializeField] public string rotateText;
    [TextArea] [SerializeField] public string stackText;
    [TextArea] [SerializeField] public string greatCheerText;
    [TextArea] [SerializeField] public string niceOneText;
    [TextArea] [SerializeField] public string needMoreAmmoText;
    [TextArea] [SerializeField] public string keepStackingText;
    [TextArea] [SerializeField] public string enemiesComingText;
    
    
    public static void SpawnFirstBlockAndTeachPlacement()
    {
       GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.RotateToPlayer(0.5f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needAmmoText, 0.5f);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);
            
            yield return new WaitForSeconds(2.75f);
            
            UIManager.THIS.speechBubble.Hide();

            Spawner.THIS.DelayedSpawn(0.0f);
            
            yield return new WaitForSeconds(0.1f);

            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.useAmmoBoxText, 0.4f);
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);

            PressOnSpawner();
        }
    }

    public static void PressOnSpawner()
    {
        Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToScreenPoint(Spawner.THIS.transform.position);
        Vector3 screenPosition = CameraManager.THIS.uiCamera.ScreenToWorldPoint(viewPort);
        UIManager.THIS.finger.OnClick = Spawner.THIS.Lift;
        UIManager.THIS.finger.ShortPressAndDrag(screenPosition, 0.75f);
    }
    
    public static void SpawnSecondBlockAndTeachRotation()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.greatCheerText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);

            yield return new WaitForSeconds(2.0f);
            
            UIManager.THIS.speechBubble.Hide();

            yield return new WaitForSeconds(0.5f);
            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.rotateText);
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);

            Vector3 viewPort = CameraManager.THIS.gameCamera.WorldToScreenPoint(Spawner.THIS.transform.position);
            Vector3 screenPosition = CameraManager.THIS.uiCamera.ScreenToWorldPoint(viewPort);
            UIManager.THIS.finger.OnClick = Spawner.THIS.Shake;
            UIManager.THIS.finger.Click(screenPosition);

            Spawner.THIS.DelayedSpawn(0.0f);

            
            yield return new WaitForSeconds(0.35f);
        }
    }
    
    public static void TalkAboutMerge()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            UIManager.THIS.speechBubble.Hide();
            
            yield return new WaitForSeconds(0.1f);
            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.stackText, 0.2f);
            
            Warzone.THIS.Player.animator.SetTrigger(Player.SHOW_HASH);

        }
    }
    
    public static void CheerForMerge()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.Replenish();
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.niceOneText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);

            yield return new WaitForSeconds(1.75f);
            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(0.25f);

            // UIManager.THIS.speechBubble.Hide();
            // yield return new WaitForSeconds(1.0f);
            
            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.enemiesComingText, 0.5f, 1.5f);
            yield return new WaitForSeconds(0.25f);
            Warzone.THIS.Begin(false);
        }
    }
    
    public static void TalkAboutNeedMoreAmmo()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(1.5f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needMoreAmmoText);
            Spawner.THIS.DelayedSpawn(0.0f);

            yield return new WaitForSeconds(2.0f);
            UIManager.THIS.speechBubble.Hide();
        }
    }
}
