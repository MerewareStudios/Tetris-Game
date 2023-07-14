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
        GameManager.THIS.DeconstructForFail();
    }

    public void LoadLevel()
    {
        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.Begin(0.45f);

        int currentLevel = this.CurrentLevel();
        UIManager.THIS.levelText.text = "Wave " + currentLevel;

        Warzone.THIS.LevelData = currentLevel.GetLevelSo();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            GameManager.PLAYING = false;
            GameManager.THIS.OnVictory();
            SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.75f, Const.PurchaseType.Coin, 125);
        }
    }

    public void CheckVictory()
    {
        if (Warzone.THIS.IsWarzoneCleared)
        {
            GameManager.PLAYING = false;
            GameManager.THIS.OnVictory();
            SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.75f, Const.PurchaseType.Coin, 125);
        }
    }
}
