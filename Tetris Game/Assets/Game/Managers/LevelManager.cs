using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public void GameOver()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        // SaveManager.THIS.SaveHighScore();
        
        GameManager.PLAYING = false;
        GameManager.THIS.Deconstruct();
    }

    public void LoadLevel()
    {
        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.Begin(0.45f);

        Warzone.THIS.LevelData = this.CurrentLevel().GetLevelSo();
        Warzone.THIS.Begin();
    }
    
    public void ReLoadLevel()
    {
        GameManager.THIS.Deconstruct();
        // ScoreBoard.THIS.Score = 0;
        Warzone.THIS.Reset();
        LoadLevel();
    }
}
