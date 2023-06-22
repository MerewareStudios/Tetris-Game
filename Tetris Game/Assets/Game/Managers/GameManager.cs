using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static MaterialPropertyBlock MPB_PAWN;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_ENEMY;
    [System.NonSerialized] public static MaterialPropertyBlock MPB_FRONT;
    [System.NonSerialized] public static bool PLAYING = false;
    private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");

    void Awake()
    {
        MPB_PAWN = new();
        MPB_ENEMY = new();
        MPB_FRONT = new();
    }
    
    void Start()
    {
        Board.THIS.Construct();
        LevelManager.THIS.LoadLevel();
    }
    
    void Update()
    {
        SaveManager.THIS.saveData.playTime += Time.deltaTime;
        Shader.SetGlobalFloat(UnscaledTime, Time.unscaledTime);
    }
    
    
    public void Deconstruct()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Warzone.THIS.Deconstruct();
    }
}
