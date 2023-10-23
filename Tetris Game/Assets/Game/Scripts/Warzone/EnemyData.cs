using System.Collections.Generic;
using Game;
using Game.UI;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Game/Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public Pool type;
    public float extraWait;
    public ParticleSystem.MinMaxGradient colorGrad;
    public Gradient hitGradient;
    public List<EnemyReward> enemyRewards;
    public int maxHealth;
    public float speed;
    public float radius;
    public int emitCount;
    public int deathEmitCount;
    public float wipeDelay;
    public float crossSize;
    public bool cast = false;


    [System.Serializable]
    public class EnemyReward
    {
        public UpgradeMenu.PurchaseType type;
        public int amount;
        [Range(0.0f, 1.0f)] public float probability;
    }
}
