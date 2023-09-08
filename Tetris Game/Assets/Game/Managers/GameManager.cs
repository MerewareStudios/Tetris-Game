using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;
using Visual.Effects;

public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static MaterialPropertyBlock MPB_PAWN;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_ENEMY;
    // [System.NonSerialized] public static MaterialPropertyBlock MPB_FRONT;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_PLACEMENT;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_DISTORTION;

    [System.NonSerialized] public static bool PLAYING = false;
    
    private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");
    public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    public static readonly int RampID = Shader.PropertyToID("_Ramp");
    public static readonly int PowerID = Shader.PropertyToID("_Power");
    public static readonly int InsideColor = Shader.PropertyToID("_InsideColor");
    public static readonly int EnemyEmisColor = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        MPB_PAWN = new();
        MPB_ENEMY = new();
        // MPB_FRONT = new();
        MPB_PLACEMENT = new();
        MPB_DISTORTION = new();
    }
    
    void Start()
    {
        Distortion.SetPropertyBlock(MPB_DISTORTION, RampID, PowerID, (go) => go.Despawn());
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
    
    // public void Deconstruct()
    // {
    //     Spawner.THIS.Deconstruct();
    //     Map.THIS.Deconstruct();
    //     Board.THIS.Deconstruct();
    //     Warzone.THIS.Deconstruct();
    // }
   
    public void OnVictory()
    {
        Spawner.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.OnVictory();
        Warzone.THIS.OnVictory();
        
        Onboarding.Deconstruct();
        CustomPower.THIS.Deconstruct();
    }
    public void OnFail()
    {
        Spawner.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.OnFail();
        Warzone.THIS.OnFail();
        
        Onboarding.Deconstruct();
        CustomPower.THIS.Deconstruct();
    }
}
