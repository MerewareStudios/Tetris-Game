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
        
        
        Audio.Powerup_Throw.PlayOneShot();
        Audio.Powerup_Rocket_Start.PlayOneShot();

        
        Sequence.onComplete = () =>
        {
            if (enemyID == enemy.ID)
            {
                enemy.TakeDamage(15, 2.0f);
            }

            Particle.Missile_Explosion.Play(hitTarget);
            Audio.Bomb_Explode.PlayOneShot();

            CameraManager.THIS.Shake(0.5f, 0.75f);
            UIManagerExtensions.QuickDistort(Position);
            
            OnDeconstruct();
        };
    }
}
