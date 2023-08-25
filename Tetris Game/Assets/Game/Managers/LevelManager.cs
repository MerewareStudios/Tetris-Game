using System;
using DG.Tweening;
using Game;
using Internal.Core;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{

    public static int CurrentLevel => LevelManager.THIS.CurrentLevel();
    public void LoadLevel()
    {
        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.OnLevelLoad();

        Warzone.THIS.LevelData = CurrentLevel.GetLevelData();
        Warzone.THIS.OnLevelLoad();
        
        // UIManager.THIS.loanBar.MakeUnavailable(10.0f);
        
        if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
        {
            Warzone.THIS.Begin(true);
            Spawner.THIS.DelayedSpawn(0.45f);
            return;
        }
        
        if (ONBOARDING.TEACH_PICK.IsNotComplete())
        {
            Onboarding.SpawnFirstBlockAndTeachPlacement();
        }
    }

    public bool CanSpawnBonus()
    {
        return this.CurrentLevel().CanSpawnBonus();
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
