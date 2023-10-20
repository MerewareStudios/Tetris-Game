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

        float duration = 0.65f;
        
        int enemyID = enemy.ID;


        Vector3 hitTarget = enemy.hitTarget.position;
        Vector3 frontPosition = hitTarget;
        frontPosition.z = Warzone.THIS.EndLine - 0.5f;

        float distanceTime = (hitTarget.z - frontPosition.z) / 6.0f;
        
        
        Tween frontTween = _transform.DOJump(frontPosition, 2.0f, 1, duration).SetEase(Ease.InOutSine);
        Tween rotateTween = _transform.DORotate(new Vector3(-120f, 180.0f, 0.0f), duration, RotateMode.Fast).SetEase(Ease.InOutSine);
        Tween moveTween = _transform.DOMove(hitTarget, distanceTime).SetEase(Ease.InBack);
        Tween rotTween = _transform.DORotate(new Vector3(0f, 0.0f, 180.0f), distanceTime * 0.75f, RotateMode.WorldAxisAdd).SetRelative(true).SetEase(Ease.Linear);

        Sequence.Append(frontTween);
        Sequence.Join(rotateTween);
        Sequence.Append(moveTween);
        Sequence.Join(rotTween);

        hitTarget.y += 2.0f;
        hitTarget.z -= 1.25f;

        enemy.DragTarget = true;
        
        Sequence.onComplete = () =>
        {
            CameraManager.THIS.Shake(1.0f, 0.75f);
            Particle.Pow.Play(hitTarget);
            if (enemyID == enemy.ID)
            {
                enemy.TakeDamage(1, 1.0f);
                enemy.Drag(2.0f, Warzone.THIS.AssignClosestEnemy);
            }
            OnDeconstruct();
        };
    }
}
