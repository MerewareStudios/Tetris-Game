using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
        Transform arrow = Pool.Arrow.Spawn<Transform>();
        arrow.DOKill();
        arrow.transform.position = barrel.position;
        Tween tween = arrow.DOJump(target, 1.0f, 1, 1.0f).SetEase(Ease.Linear);
        Vector3 prev = arrow.position;
        tween.onUpdate += () =>
        {
            arrow.forward = Vector3.Lerp(arrow.forward, (arrow.position - prev), Time.deltaTime * 20.0f);
            prev = arrow.position;
        };
        tween.onComplete += () =>
        {
            OnHit.Invoke();
            arrow.gameObject.Despawn();
        };
        
    }
}
