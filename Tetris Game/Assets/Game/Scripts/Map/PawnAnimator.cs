using UnityEngine;

public class PawnAnimator : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private Transform barrel;
    [System.NonSerialized] private static int ATTACK_HASH = Animator.StringToHash("Attack");

    public void Attack(Vector3 target, System.Action OnHit)
    {
        _animator.SetTrigger(ATTACK_HASH);
        Projectile(target, OnHit);
    }

    private void Projectile(Vector3 target, System.Action OnHit)
    {
        
    }
}
