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

        UIManager.THIS.levelText.enabled = ONBOARDING.LEARNED_LEVEL_TEXT.IsComplete();
        UIManager.THIS.levelText.text = "LEVEL " + CurrentLevel;
        
        Warzone.THIS.EnemySpawnData = CurrentLevel.GetEnemySpawnData();
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

    public void CheckEndLevel()
    {
        if (Warzone.THIS.Player._CurrentHealth <= 0)
        {
            OnFail();
        }
        if (Warzone.THIS.IsCleared)
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

        
        SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.25f, this.CurrentLevel().GetVictoryReward());
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
