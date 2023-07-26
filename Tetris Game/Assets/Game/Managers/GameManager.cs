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
    [System.NonSerialized] public static MaterialPropertyBlock MPB_FRONT;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_PLACEMENT;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_DISTORTION;

    [System.NonSerialized] public static bool PLAYING = false;
    [SerializeField] public Transform pos;
    
    private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");
    public static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    public static readonly int DistortionDistance = Shader.PropertyToID("_Ramp");

    void Awake()
    {
        MPB_PAWN = new();
        MPB_ENEMY = new();
        MPB_FRONT = new();
        MPB_PLACEMENT = new();
        MPB_DISTORTION = new();
    }
    
    void Start()
    {
        Distortion.SetPropertyBlock(MPB_DISTORTION, DistortionDistance);
        Board.THIS.Construct();
        Play();
    }

    public void Play()
    {
        LevelManager.THIS.LoadLevel();
    }
    
    void Update()
    {
        SaveManager.THIS.saveData.playTime += Time.deltaTime;
        if (UIManager.MENU_VISIBLE)
        {
            Shader.SetGlobalFloat(UnscaledTime, Time.unscaledTime);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            Distortion distortion = Pool.Distortion.Spawn<Distortion>();
            distortion.Distort(pos.position, pos.forward, Vector3.one * 5.0f, 0.8f);
        }
    }
    
    
    public void Deconstruct()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Warzone.THIS.Deconstruct();
    }
   
    public void OnVictory()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.OnVictory();
        Warzone.THIS.OnVictory();
    }
    public void OnFail()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.OnFail();
        Warzone.THIS.OnFail();
    }
}
