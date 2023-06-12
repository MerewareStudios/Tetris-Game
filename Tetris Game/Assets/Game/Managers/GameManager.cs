using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [System.NonSerialized] public static MaterialPropertyBlock MPB_PAWN;
    [System.NonSerialized] public static bool GAME_OVER = true;
    
    void Awake()
    {
        MPB_PAWN = new();
    }
    
    void Start()
    {
        Board.THIS.Construct();
        LevelManager.THIS.LoadLevel();
    }
    
    
    public void Deconstruct()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Board.THIS.Deconstruct();
        Warzone.THIS.Deconstruct();
    }
}
