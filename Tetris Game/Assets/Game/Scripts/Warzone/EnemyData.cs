using System.Collections.Generic;
using Game;
using Game.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Game/Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public Enemy.Type type;
    public Color color;
    public Gradient hitGradient;
    public List<EnemyReward> enemyRewards;
    public int damage;
    public int maxHealth;
    public float speed;
    public float radius;
    public int emitCount;
    public int deathEmitCount;
    public float wipeDelay;


    [System.Serializable]
    public class EnemyReward
    {
        public UpgradeMenu.PurchaseType type;
        public int rewardAmount;
        [Range(0.0f, 1.0f)] public float rewardProbability;
    }
}
