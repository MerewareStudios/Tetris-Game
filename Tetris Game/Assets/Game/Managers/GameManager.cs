using Game;
using Internal.Core;
using UnityEngine;
using Visual.Effects;

public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static bool PLAYING = false;
    
    private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");
    public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    public static readonly int RampID = Shader.PropertyToID("_Ramp");
    public static readonly int PowerID = Shader.PropertyToID("_Power");
    public static readonly int InsideColor = Shader.PropertyToID("_InsideColor");
    public static readonly int EnemyEmisColor = Shader.PropertyToID("_EmissionColor");

    void Start()
    {
        Distortion.SetPropertyBlock(RampID, PowerID, (go) => go.Despawn());
        Board.THIS.Construct();
        LevelManager.THIS.LoadLevel();
    }

    void Update()
    {
        SaveManager.THIS.saveData.playTime += Time.deltaTime;

        if (UIManager.MENU_VISIBLE)
        {
            Shader.SetGlobalFloat(UnscaledTime, Time.unscaledTime);
        }
    }
    
    public static void AddCoin(int value)
    {
       Wallet.COIN.Transaction(value);
    }
    public static void AddHeart(int value)
    {
        Warzone.THIS.Player._CurrentHealth += value;
    }
    public static void AddShield(int value)
    {
        Warzone.THIS.Player.shield.Add(value);
    }
    
    // public void Deconstruct()
    // {
    //     Spawner.THIS.Deconstruct();
    //     Map.THIS.Deconstruct();
    //     Board.THIS.Deconstruct();
    //     Warzone.THIS.Deconstruct();
    // }
   
    public void Deconstruct()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Warzone.THIS.Deconstruct();
        Onboarding.Deconstruct();
        CustomPower.THIS.Deconstruct();
    }

    public void OnVictory()
    {
        Warzone.THIS.OnVictory();
        OnLevelEnd();
    }
    public void OnFail()
    {
        Warzone.THIS.OnFail();
        OnLevelEnd();
    }

    private void OnLevelEnd()
    {
#if UNITY_EDITOR
        Debug.Log(Time.time - LevelManager.LevelStart);
#endif

        Board.THIS.OnLevelEnd();
        Map.THIS.OnLevelEnd();
        Spawner.THIS.OnLevelEnd();
    }
}
