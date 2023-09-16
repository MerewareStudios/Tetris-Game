using System.Collections;
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
    [TextArea] [SerializeField] public string enemiesComingText;
    [TextArea] [SerializeField] public string ticketMergeText;
    [Header("Other")]
    [TextArea] [SerializeField] public string waveText;
    [Header("Block Menu")]
    [TextArea] [SerializeField] public string fullText;
    [TextArea] [SerializeField] public string plusText;
    [TextArea] [SerializeField] public string damageText;
    [TextArea] [SerializeField] public string fireRateText;
    [TextArea] [SerializeField] public string splitShotText;
    
    public Coroutine coroutine = null;

    public static void Deconstruct()
    {
        Onboarding.StopRoutine();
        
        UIManager.THIS.speechBubble.Hide();
        HideFinger();
    }
    
    public static void StopRoutine()
    {
        if (Onboarding.THIS.coroutine != null)
        {
            GameManager.THIS.StopCoroutine(Onboarding.THIS.coroutine);
            Onboarding.THIS.coroutine = null;
        }
    }
    
    public static void SpawnFirstBlockAndTeachPlacement()
    {
       GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.RotateToPlayer(0.5f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needAmmoText, 0.35f);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);
            
            yield return new WaitForSeconds(2.25f);
            
            UIManager.THIS.speechBubble.Hide();

            Spawner.THIS.DelayedSpawn(0.0f);
            
            yield return new WaitForSeconds(0.1f);

            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.useAmmoBoxText, 0.4f);
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);

            DragOn(Spawner.THIS.transform.position, true, Spawner.THIS.Lift);
        }
    }

    public static void DragOn(Vector3 position, bool worldSpace, System.Action OnClick)
    {
        UIManager.THIS.finger.OnClick = OnClick;
        UIManager.THIS.finger.ShortPressAndDrag(position, worldSpace, 0.75f);
    }
    public static void ClickOn(Vector3 position, bool worldSpace, System.Action OnClick)
    {
        UIManager.THIS.finger.OnClick = OnClick;
        UIManager.THIS.finger.Click(position, worldSpace);
    }
    public static void HideFinger()
    {
        UIManager.THIS.finger.Hide();
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

            ClickOn(Spawner.THIS.transform.position, true, Spawner.THIS.Shake);

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
    public static void TalkAboutTicketMerge()
    {
        Onboarding.THIS.coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.1f);
            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.ticketMergeText, 0.2f);

            yield return new WaitForSeconds(0.25f);
            Onboarding.ClickOn(CustomPower.THIS.clickTarget.position, true, CustomPower.THIS.HighlightPunch);
        }
    }
    
    public static void CheerForMerge()
    {
        Onboarding.THIS.coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.Replenish();
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.niceOneText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);

            yield return new WaitForSeconds(1.25f);
            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(0.25f);

            // UIManager.THIS.speechBubble.Hide();
            // yield return new WaitForSeconds(1.0f);
            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.enemiesComingText, 0.5f, 1.25f);
            ONBOARDING.LEARNED_LEVEL_TEXT.SetComplete();
            UIManager.THIS.levelText.enabled = true;
            yield return new WaitForSeconds(0.25f);
            Warzone.THIS.Begin(false);
            
            ONBOARDING.LEARNED_META.SetComplete();
            ONBOARDING.INSPECT_HEART_DISPLAY.SetComplete();
            StatDisplayArranger.THIS.Show(StatDisplay.Type.Health, Warzone.THIS.Player._CurrentHealth);
        }
    }
    
    public static void TalkAboutNeedMoreAmmo()
    {
        GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(1.25f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needMoreAmmoText);
            Spawner.THIS.DelayedSpawn(0.0f);

            
            

            yield return new WaitForSeconds(2.0f);
            UIManager.THIS.speechBubble.Hide();
        }
    }
}
