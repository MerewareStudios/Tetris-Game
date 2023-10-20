using DG.Tweening;
using Game;
using UnityEngine;

public class Rocket : SubModel
{
    public override void OnProjectile(Enemy enemy)
    {
        base.OnProjectile(enemy);
        
        RefreshSequence();
        
        if (!enemy)
        {
            return;
        }


        int enemyID = enemy.ID;

        Vector3 hitTarget = enemy.hitTarget.position;

        Tween jumpTween = ThisTransform.DOJump(hitTarget, AnimConst.THIS.missileJumpPower, 1, AnimConst.THIS.missileDuration).SetEase(AnimConst.THIS.missileEase, AnimConst.THIS.missileOvershoot);
        
        Sequence.Append(jumpTween);

        
        Vector3 lastPos = ThisTransform.position;
        jumpTween.onUpdate = () =>
        {
            var current = ThisTransform.position;
            Quaternion target = Quaternion.LookRotation((current - lastPos).normalized, ThisTransform.up) * Quaternion.Euler(90.0f, 0.0f, 0.0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, target, Time.deltaTime * jumpTween.ElapsedPercentage() * 80.0f);
            lastPos = current;
        };
        
        Sequence.onComplete = () =>
        {
            if (enemyID == enemy.ID)
            {
                enemy.TakeDamage(15, 2.0f);
            }

            Particle.Missile_Explosion.Play(hitTarget);
            CameraManager.THIS.Shake(0.2f, 0.5f);
            UIManagerExtensions.Distort(Position, 0.0f);
            
            OnDeconstruct();
        };
    }
}
