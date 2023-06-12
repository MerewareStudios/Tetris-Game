using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public void GameOver()
    {
        if (GameManager.GAME_OVER)
        {
            return;
        }
        Debug.Log("Game Over");
        GameManager.GAME_OVER = true;
        GameManager.THIS.Deconstruct();
    }

    public void LoadLevel()
    {
        GameManager.GAME_OVER = false;
        Map.THIS.StartMainLoop();
        Spawner.THIS.Begin();
        Warzone.THIS.StartSpawning();
    }
}
