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
        Spawner.THIS.OnLevelLoad();

        UIManager.THIS.levelText.text = "Level " + this.CurrentLevel();

        Warzone.THIS.LevelData = this.CurrentLevel().GetLevelData();
        Warzone.THIS.OnLevelLoad();
        
        UIManager.THIS.loanBar.MakeUnavailable(10.0f);
        
        if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
        {
            Spawner.THIS.DelayedSpawn(0.45f);
            return;
        }
        
        if (ONBOARDING.FIRST_BLOCK_SPAWN_AND_PLACE.IsNotComplete())
        {
            Onboarding.SpawnFirstBlockAndTeachPlacement();
        }
    }

    public Board.SuggestedBlock[] GetSuggestedBlocks()
    {
        return this.CurrentLevel().GetSuggestedBlocks();
    }

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

    public void OnVictory()
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
    
    public void OnFail()
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
