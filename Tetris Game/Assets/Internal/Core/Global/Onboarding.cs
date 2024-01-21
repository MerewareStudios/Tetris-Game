using System.Collections;
using Game;
using IWI.Tutorial;
using UnityEngine;

[CreateAssetMenu(fileName = "Onboarding Data", menuName = "Game/Onboarding Data", order = 0)]
public class Onboarding : SSingleton<Onboarding>
{
    [TextArea] [SerializeField] public string needAmmoText;
    [TextArea] [SerializeField] public string stackText;
    [TextArea] [SerializeField] public string greatCheerText;
    [TextArea] [SerializeField] public string niceOneText;
    [TextArea] [SerializeField] public string needMoreAmmoText;
    [TextArea] [SerializeField] public string targetPracticeText;
    [TextArea] [SerializeField] public string enemiesComingText;
    [TextArea] [SerializeField] public string ticketMergeText;
    [TextArea] [SerializeField] public string freePlacementText;
    [Header("Air Drop")]
    [TextArea] [SerializeField] public string maxStackDropCheer;
    [TextArea] [SerializeField] public string healthDropCheer;
    [TextArea] [SerializeField] public string chestDropCheer;
    [TextArea] [SerializeField] public string airDropFull;
    [Header("Other")]
    [TextArea] [SerializeField] public string waveText;
    [Header("Block Menu")]
    [TextArea] [SerializeField] public string fullText;
    [TextArea] [SerializeField] public string plusText;
    [TextArea] [SerializeField] public string damageText;
    [TextArea] [SerializeField] public string fireRateText;
    [TextArea] [SerializeField] public string splitShotText;
    [TextArea] [SerializeField] public string equippedText;
    [TextArea] [SerializeField] public string hasText;
    [TextArea] [SerializeField] public string unlockedAtText;
    [Header("Ad Break")]
    [TextArea] [SerializeField] public string useTicketText;
    [TextArea] [SerializeField] public string earnTicketText;
    [TextArea] [SerializeField] public string skipButtonText;
    [TextArea] [SerializeField] public string cancelButtonText;
    [SerializeField] public AdBreakScreen.VisualData adBreakVisualData;
    [SerializeField] public AdBreakScreen.VisualData rewardedAdVisualData;
    [Header("Weapon Stat")]
    [TextArea] [SerializeField] public string weaponStatUnchange;
    [TextArea] [SerializeField] public string weaponStatChange;
    [Header("Tips")]
    [TextArea] [SerializeField] public string[] tips;
    [TextArea] [SerializeField] public string commentTip;
    [TextArea] [SerializeField] public string shareTip;
    [TextArea] [SerializeField] public string reviewText;
    [TextArea] [SerializeField] public string shareText;
    [TextArea] [SerializeField] public string thanksText;
    
    
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
    
    public static void SpawnBlockAndTeachPlacement()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.2f);

            
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.greatCheerText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);
            
            yield return new WaitForSeconds(1.15f);
            
            UIManager.THIS.speechBubble.Hide();
            DragOn(Spawner.THIS.transform.position, Finger.Cam.Game, null, timeIndependent:false);

            
            yield return new WaitForSeconds(0.55f);
            
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);
            
            
            // Spawner.THIS.Spawn();
            // Audio.Hint_1.Play();
            

            


            

        }
    }
    public static void SpawnBlockAndTeachRotation()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.2f);
            
            Warzone.THIS.Player.RotateToPlayer(0.5f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needAmmoText, 0.35f);
            Warzone.THIS.Player.animator.SetTrigger(Player.WAVE_HASH);
            
            Spawner.THIS.PlaceBlock(Pool.S_Block, new Vector2Int(0, 0));
            Spawner.THIS.PlaceBlock(Pool.Z_Block, new Vector2Int(4, 0));
            yield return new WaitForSeconds(1.5f);
            
            UIManager.THIS.speechBubble.Hide();
            
            yield return new WaitForSeconds(0.1f);

            Spawner.THIS.Spawn();
            Audio.Hint_1.Play();

            yield return new WaitForSeconds(0.1f);
            
            Warzone.THIS.Player.animator.SetTrigger(Player.POINT_HASH);
            
            ClickOn(Spawner.THIS.transform.position, Finger.Cam.Game, Spawner.THIS.RotateSelf, infoEnabled:true, timeIndependent:false);
            
            
        }
    }

    public static void DragOn(Vector3 position, Finger.Cam rendererCamera, System.Action OnClick, float scale = 1.0f, bool timeIndependent = true)
    {
        UIManager.THIS.finger.OnClick = OnClick;
        UIManager.THIS.finger.ShortPressAndDrag(position, rendererCamera, scale, timeIndependent);
    }
    public static void ClickOn(Vector3 position, Finger.Cam rendererCamera, System.Action OnClick, float scale = 0.75f, bool infoEnabled = false, bool timeIndependent = true)
    {
        UIManager.THIS.finger.OnClick = OnClick;
        UIManager.THIS.finger.Click(position, rendererCamera, scale, infoEnabled, timeIndependent);
    }
    public static void HideFinger()
    {
        UIManager.THIS.finger.Hide();
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
        }
    }
    public static void TalkAboutPowerUp()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.1f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.ticketMergeText, 0.2f);

            ClickOn(Powerup.THIS.fingerTarget.position, Finger.Cam.Game, () => Powerup.THIS.PunchFrame(0.4f), 0.7f, timeIndependent:false);
        }
    }
    
    public static void CheerForMerge()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            Warzone.THIS.Player.Replenish(6);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.niceOneText, 0.15f);
            Warzone.THIS.Player.animator.SetTrigger(Player.VICTORY_INF_HASH);

            yield return new WaitForSeconds(1.0f);
            UIManager.THIS.speechBubble.Hide();
            yield return new WaitForSeconds(0.25f);


            ONBOARDING.PASSIVE_META.SetComplete();
            LevelManager.THIS.levelText.enabled = true;
            Warzone.THIS.enemyProgressbar.Visible = true;
            StatDisplayArranger.THIS.Show(StatDisplay.Type.Health, Warzone.THIS.Player._CurrentHealth);
            Warzone.THIS.Begin();
        }
    }
    
    public static void TalkAboutNeedMoreAmmo()
    {
        Onboarding.THIS.Coroutine = GameManager.THIS.StartCoroutine(Routine());

        IEnumerator Routine()
        {
            yield return new WaitForSeconds(0.75f);
            UIManager.THIS.speechBubble.Speak(Onboarding.THIS.needMoreAmmoText);
            Spawner.THIS.Spawn();
            Audio.Hint_1.Play();

            yield return new WaitForSeconds(2.0f);
            UIManager.THIS.speechBubble.Hide();
        }
    }
}
