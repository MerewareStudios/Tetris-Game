using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public void OnWictory()
    {
        Spawner.THIS.Deconstruct();
        Map.THIS.Deconstruct();
        Warzone.THIS.Deconstruct();
        LoadLevel();
    }
    
    public void GameOver()
    {
        if (GameManager.GAME_OVER)
        {
            return;
        }
        Debug.Log("Game Over");
        GameManager.GAME_OVER = true;
        // Spawner.THIS.Deconstruct();
        // Map.THIS.Deconstruct();
        // Warzone.THIS.Deconstruct();
        // LoadLevel();
    }

    public void LoadLevel()
    {
        GameManager.GAME_OVER = false;
        Map.THIS.Begin();
        Spawner.THIS.Begin();
        Warzone.THIS.StartSpawning();
    }
}
