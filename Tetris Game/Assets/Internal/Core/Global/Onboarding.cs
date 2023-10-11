using System.Collections;
using Game;
using Game.UI;
using Internal.Core;
using IWI.Tutorial;
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
    [TextArea] [SerializeField] public string freePlacementText;
    [Header("Other")]
    [TextArea] [SerializeField] public string waveText;
    [Header("Block Menu")]
    [TextArea] [SerializeField] public string fullText;
    [TextArea] [SerializeField] public string plusText;
    [TextArea] [SerializeField] public string damageText;
    [TextArea] [SerializeField] public string fireRateText;
    [TextArea] [SerializeField] public string splitShotText;
    [Header("Ad Break")]
    [TextArea] [SerializeField] public string adBreakText;
    [TextArea] [SerializeField] public string earnText;
    [TextArea] [SerializeField] public string useTicketText;
    [TextArea] [SerializeField] public string earnTicketText;
    [TextArea] [SerializeField] public string skipButtonText;
    [TextArea] [SerializeField] public string cancelButtonText;
    [Header("Stat Color")]
    [SerializeField] public Color specialStatColor;
    [SerializeField] public Color normalStatColor;
    [Header("Weapon Stat")]
    [TextArea] [SerializeField] public string weaponStatUnchange;
    [TextArea] [SerializeField] public string weaponStatChange;
    
    public Coroutine Coroutine = null;

    public static void Deconstruct()
    {
        Onboarding.StopRoutine();
        
        UIManager.THIS.speechBubble.Hide();
        HideFinger();
    }
    
    public static void StopRoutine()
    {
        if (Onboarding.THIS.Coroutine != null)
        {
            GameManager.THIS.StopCoroutine(Onboarding.THIS.Coroutine);
            Onboarding.THIS.Coroutine = null;
        }
    }
    
    public static void SpawnFirstBlockAndTeachPlacement()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.RotateToPlayer(0.5f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needAmmoText, 0.35f);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);
            
            yield return new WaitForSeconds(2.25f);
            
            UIManager.THIS.speechBubble.Hide();

            Spawner.THIS.DelayedSpawn(0.0f);
            
            yield return new WaitForSeconds(0.1f);

            // UIManager.THIS.speechBubble.Speak(Onboarding.THIS.useAmmoBoxText, 0.4f);
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);

            DragOn(Spawner.THIS.transform.position, Finger.Cam.Game, Spawner.THIS.Lift);
        }
    }

    public static void DragOn(Vector3 position, Finger.Cam rendererCamera, System.Action OnClick, float scale = 1.0f)
    {
        UIManager.THIS.finger.OnClick = OnClick;
        UIManager.THIS.finger.ShortPressAndDrag(position, rendererCamera, scale);
    }
    public static void ClickOn(Vector3 position, Finger.Cam rendererCamera, System.Action OnClick, float scale = 1.0f, bool infoEnabled = false)
    {
        UIManager.THIS.finger.OnClick = OnClick;
        UIManager.THIS.finger.Click(position, rendererCamera, scale, infoEnabled);
    }
    public static void HideFinger()
    {
        UIManager.THIS.finger.Hide();
    }
    
    public static void SpawnSecondBlockAndTeachRotation()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.greatCheerText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);

            yield return new WaitForSeconds(2.0f);
            
            UIManager.THIS.speechBubble.Hide();

            yield return new WaitForSeconds(0.65f);
            
            // UIManager.THIS.speechBubble.Speak(Onboarding.THIS.rotateText);
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);

            ClickOn(Spawner.THIS.transform.position, Finger.Cam.Game, Spawner.THIS.Shake, infoEnabled:true);

            Spawner.THIS.DelayedSpawn(0.0f);

            
            yield return new WaitForSeconds(0.35f);
        }
    }
    
    public static void TalkAboutMerge()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            UIManager.THIS.speechBubble.Hide();
            
            yield return new WaitForSeconds(0.1f);
            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.stackText, 0.2f);
            
            Warzone.THIS.Player.animator.SetTrigger(Player.SHOW_HASH);
        }
    }
    public static void TalkAboutFreePlacement()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(0.1f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.freePlacementText, 0.2f);
            yield return new WaitForSeconds(2.5f);
            UIManager.THIS.speechBubble.Hide();
            ONBOARDING.PLACE_POWERUP.SetComplete();
        }
    }
    public static void TalkAboutPowerUp()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.1f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.ticketMergeText, 0.2f);

            // yield return new WaitForSeconds(0.25f);
            ClickOn(Powerup.THIS.fingerTarget.position, Finger.Cam.Game, () => Powerup.THIS.PunchFrame(0.4f), 0.7f);
        }
    }
    
    public static void CheerForMerge()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

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
            
            // UIManager.THIS.speechBubble.Speak(Onboarding.THIS.enemiesComingText, 0.5f, 1.5f);
            Announcer.THIS.Show("Target Practice", 0.5f);

            ONBOARDING.LEARNED_LEVEL_VISUALS.SetComplete();
            UIManager.THIS.levelText.enabled = true;
            UIManager.THIS.levelProgressbar.SetActive(true);
            UIManager.THIS.levelProgress.fillAmount = 1.0f;
            yield return new WaitForSeconds(1.15f);
            Warzone.THIS.Begin();
            
            ONBOARDING.LEARNED_META.SetComplete();
            ONBOARDING.INSPECT_HEART_DISPLAY.SetComplete();
            StatDisplayArranger.THIS.Show(StatDisplay.Type.Health, Warzone.THIS.Player._CurrentHealth);
        }
    }
    
    public static void TalkAboutNeedMoreAmmo()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.75f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needMoreAmmoText);
            Spawner.THIS.DelayedSpawn(0.0f);

            yield return new WaitForSeconds(2.0f);
            UIManager.THIS.speechBubble.Hide();
        }
    }
}
