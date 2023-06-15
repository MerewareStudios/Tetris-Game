using System;
using Game;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data", order = 0)]
    public class LevelSo : ScriptableObject
    {
        [SerializeField] public Level.Data LevelData;
    }
}



public static class LevelSoExtension
{
    public static Level.Data GetLevelSo(this int level)
    {
        return Const.THIS.Levels[level - 1].LevelData;
    } 
}