using Game;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy Data", menuName = "Game/Enemy Data", order = 0)]
public class EnemyData : ScriptableObject
{
    public Enemy.Type type;
    public Color color;
    public Gradient hitGradient;
    public int maxHealth;
    public float speed;
    public float radius;
}
