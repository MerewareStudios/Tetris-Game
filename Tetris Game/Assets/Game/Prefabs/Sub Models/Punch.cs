using DG.Tweening;
using Game;
using UnityEngine;

public class Punch : SubModel
{
    public override void OnProjectile(Enemy enemy)
    {
        base.OnProjectile(enemy);

        if (!enemy)
        {
            OnDeconstruct();
            return;
        }
        
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();

        float duration = 0.75f;
        
        int enemyID = enemy.ID;


        Vector3 hitTarget = enemy.hitTarget.position;
        Vector3 frontPosition = hitTarget;
        frontPosition.z = Warzone.THIS.EndLine;
        
        
        Tween frontTween = _transform.DOJump(frontPosition, 5.0f, 1, duration).SetEase(Ease.InSine);
        Tween rotateTween = _transform.DORotate(new Vector3(-120f, 180.0f, 0.0f), duration).SetEase(Ease.OutBack);
        Tween moveTween = _transform.DOMove(hitTarget, 0.3f).SetEase(Ease.InBack);

        Sequence.Append(frontTween);
        Sequence.Join(rotateTween);
        Sequence.Append(moveTween);

        hitTarget.y += 2.0f;
        hitTarget.z -= 1.25f;
        
        Sequence.onComplete = () =>
        {
            CameraManager.THIS.Shake(1.0f, 0.75f);
            Particle.Pow.Play(hitTarget);
            if (enemyID == enemy.ID)
            {
                enemy.TakeDamage(1, 1.0f);
                enemy.Drag(1.5f, Warzone.THIS.AssignClosestEnemy);
            }
            OnDeconstruct();
        };
    }
}
