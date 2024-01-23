using DG.Tweening;
using Game;
using UnityEngine;

public class Punch : SubModel
{
    public override void OnProjectile(Enemy enemy)
    {
        base.OnProjectile(enemy);

        RefreshSequence();
        
        // if (!enemy)
        // {
        //     return;
        // }

        bool validEnemy = enemy;
        Vector3 hitTarget;
        int enemyID = -1;

        if (validEnemy)
        {
            enemyID = enemy.ID;
            hitTarget = enemy.hitTarget.position;
            enemy.DragTarget = true;
        }
        else
        {
            hitTarget = Warzone.THIS.RandomInvalidForwardPosition();
        }

        const float duration = 0.65f;

        Vector3 frontPosition = hitTarget;
        frontPosition.z = Warzone.THIS.EndLine - 0.5f;

        float distanceTime = (hitTarget.z - frontPosition.z) / 9.0f;
        
        
        Tween frontTween = ThisTransform.DOJump(frontPosition, 2.0f, 1, duration).SetEase(Ease.InOutSine);
        Tween rotateTween = ThisTransform.DORotate(new Vector3(-120f, 180.0f, 0.0f), duration, RotateMode.Fast).SetEase(Ease.InOutSine);
        Tween moveTween = ThisTransform.DOMove(hitTarget, distanceTime).SetEase(Ease.InBack, 2.0f);
        Tween rotTween = ThisTransform.DORotate(new Vector3(0f, 0.0f, 180.0f), distanceTime * 0.75f, RotateMode.WorldAxisAdd).SetRelative(true).SetEase(Ease.Linear);

        Sequence.Append(frontTween);
        Sequence.Join(rotateTween);
        Sequence.Append(moveTween);
        Sequence.Join(rotTween);

        hitTarget.y += 2.0f;
        hitTarget.z -= 1.25f;

        
        Audio.Powerup_Throw.PlayOneShot();
        
        Sequence.onComplete = () =>
        {
            if (validEnemy)
            {
                if (enemyID == enemy.ID)
                {
                    enemy.TakeDamage(10, 1.0f);
                    enemy.Drag(2.0f, Warzone.THIS.AssignClosestEnemy);
                }
                
                CameraManager.THIS.Shake(0.75f, 0.75f);
                Particle.Pow.Play(hitTarget);
                Audio.Punch.PlayOneShot();
            }
            else
            {
                Particle.Miss.Play(hitTarget);
                Audio.Miss.PlayOneShot();
            }
            OnDeconstruct();
        };
    }
}
