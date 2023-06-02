using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{

    // [System.NonSerialized] private Pawn winnerPawn = null;

    // public bool GameOver
    // {
    //     get
    //     {
    //         return winnerPawn != null;
    //     }
    // } 
    // public bool Wictory
    // {
    //     get
    //     {
    //         return Map.THIS;
    //     }
    // }

    void Start()
    {
        DOVirtual.DelayedCall(1.0f, LoadLevel);
    }

    // public void CheckWinCondition(Pawn pawn)
    // {
    //     if (pawn.Level >= Map.THIS.line.pawnBig.Level)
    //     {
    //         this.winnerPawn = pawn;
    //     }
    //      
    // }
    
    public void OnWictory()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Map.THIS.line.Deconstruct();
        // winnerPawn = null;
        LoadLevel();
    }

    public void LoadLevel()
    {
        Map.THIS.line.Construct(Map.THIS.grid.size.x, 150);
        Map.THIS.Begin();
        Spawner.THIS.Begin();
    }
}
