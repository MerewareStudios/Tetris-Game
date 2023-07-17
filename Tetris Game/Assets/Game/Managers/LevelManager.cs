using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    public void LoadLevel()
    {
        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.Begin(0.45f);

        UIManager.THIS.levelText.text = "Level " + this.CurrentLevel();

        Warzone.THIS.LevelData = this.CurrentLevel().GetLevelSo();
        Warzone.THIS.OnLevelLoad();
        
        UIManager.THIS.loanBar.MakeUnavailable(10.0f);
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            OnVictory();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnFail();
        }
    }
#endif

    public void CheckVictory()
    {
        if (Warzone.THIS.IsWarzoneCleared)
        {
            OnVictory();
        }
    }
    
    public void CheckFail()
    {
        OnFail();
    }

    private void OnVictory()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        
        GameManager.PLAYING = false;
        GameManager.THIS.OnVictory();

        SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.75f, this.CurrentLevel().GetVictoryReward());
        
        this.NextLevel();
    }
    
    private void OnFail()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        
        GameManager.PLAYING = false;
        GameManager.THIS.OnFail();
        SlashScreen.THIS.Show(SlashScreen.State.Fail, 0.25f, this.CurrentLevel().GetFailReward());
    }
}
