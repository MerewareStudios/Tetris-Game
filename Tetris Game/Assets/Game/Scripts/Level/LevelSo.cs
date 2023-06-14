using System;
using Game;
using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "Level", menuName = "Game/Level Data", order = 0)]
    public class LevelSo : ScriptableObject
    {
        [SerializeField] public float totalDuration = 10.0f;
        [SerializeField] public float spawnDelay = 0.0f;
        [SerializeField] public int totalHealth = 100;
        [SerializeField] public int maxMerge = 2;
        [SerializeField] public float speed = 1.0f;
        [SerializeField] public float spawnWidth = 2.5f;
        
        [SerializeField] public ParticleSystem.MinMaxCurve spawnRate;
    }
}



public static class LevelSoExtension
{
    public static LevelSo GetLevelSo(this int level)
    {
        return Const.THIS.Levels[level - 1];
    } 
}