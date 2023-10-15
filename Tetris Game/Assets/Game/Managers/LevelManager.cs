using Game;
using Internal.Core;
using IWI;

public class LevelManager : Singleton<LevelManager>
{
    public static int CurrentLevel => LevelManager.THIS.CurrentLevel();
    public static float DeltaMult = 1.0f;

    public void LoadLevel()
    {
        Board.THIS.Construct(CurrentLevel.BoardSize());

        GameManager.PLAYING = true;
        Map.THIS.StartMainLoop();
        Spawner.THIS.OnLevelLoad();

        UIManager.THIS.levelText.text = "LEVEL " + CurrentLevel;
        LevelManager.DeltaMult = CurrentLevel.DeltaMult();

        Warzone.THIS.EnemySpawnData = CurrentLevel.GetEnemySpawnData();
        Warzone.THIS.OnLevelLoad();
        
        if (ONBOARDING.ALL_BLOCK_STEPS.IsComplete())
        {
            Warzone.THIS.Begin();
            Spawner.THIS.DelayedSpawn(0.45f);
        }
        else if (ONBOARDING.TEACH_PICK.IsNotComplete())
        {
            Onboarding.SpawnFirstBlockAndTeachPlacement();
        }

        if (CurrentLevel >= 3)
        {
            AdManager.THIS.ShowBannerOrOffer();
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
        if (!GameManager.PLAYING)
        {
            return;
        }
        if (Warzone.THIS.Player._CurrentHealth <= 0)
        {
            OnFail();
            return;
        }
        if (Warzone.THIS.IsCleared)
        {
            OnVictory();
        }
    }

    public void OnVictory()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnVictory();
        SlashScreen.THIS.Show(SlashScreen.State.Victory, 0.25f, this.CurrentLevel().GetVictoryReward());
        this.NextLevel();
    }
    
    public void OnFail()
    {
        GameManager.PLAYING = false;
        
        GameManager.THIS.OnFail();
        SlashScreen.THIS.Show(SlashScreen.State.Fail, 0.25f, this.CurrentLevel().GetFailReward());
    }
}
