using System.Collections.Generic;
using Game;
using Game.UI;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Game/Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public Pool type;
    public Vector3 scale = Vector3.one;
    public float extraWait;
    public ParticleSystem.MinMaxGradient colorGrad;
    public Gradient hitGradient;
    public List<EnemyReward> enemyRewards;
    public int maxHealth;
    public float speed;
    public Vector2 forwardRange = new Vector2(0.0f, 1.0f);
    public float radius;
    public int emitCount;
    public int deathEmitCount;
    public float wipeDelay;
    public float crossSize;
    public Enemy.CastTypes castType;
    public Enemy.DeathAction deathAction;
    [FormerlySerializedAs("extraInt")] public int spawnerCount = 0;
    [FormerlySerializedAs("extraFloat")] public float spawnerDuration = 0;
    public int spawnerExtra = 0;
    public EnemyData extraData;

    public float RandomForwardRange() => Random.Range(forwardRange.x, forwardRange.y);

    [System.Serializable]
    public class EnemyReward
    {
        public Enemy.EnemyReward type;
        public int amount;
        [Range(0.0f, 1.0f)] public float probability;
    }
}
