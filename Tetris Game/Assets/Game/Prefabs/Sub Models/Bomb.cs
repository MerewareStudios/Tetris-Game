using DG.Tweening;
using Game;
using UnityEngine;

public class Bomb : SubModel
{
    [SerializeField] private CircularProgress progress;
    [System.NonSerialized] private float _current;

    public override void OnConstruct(Transform customParent)
    {
        base.OnConstruct(customParent);
        
        StartTimer();
    }

    public override void OnDeconstruct()
    {
        base.OnDeconstruct();
        StopTimer();
    }

    public override void OnProjectile(Enemy enemy)
    {
        base.OnProjectile(enemy);
        
        StopTimer();
        RefreshSequence();
        
        
        if (!enemy)
        {
            return;
        }


        // int enemyID = enemy.ID;

        Vector3 hitTarget = enemy.hitTarget.position;


        Tween jumpTween = ThisTransform.DOJump(hitTarget, 4.0f, 1, 0.75f).SetEase(Ease.InSine);
        Tween rotateTween = ThisTransform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), 0.75f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetRelative(true);

        Sequence.Join(jumpTween);
        Sequence.Join(rotateTween);

        Sequence.onComplete = () =>
        {
            // if (enemyID == enemy.ID)

            Warzone.THIS.AEODamage(hitTarget, 15, 2.0f);
            
            Particle.Missile_Explosion.Play(hitTarget);
            CameraManager.THIS.Shake(0.2f, 0.5f);
            UIManagerExtensions.Distort(Position, 0.0f);
            
            OnDeconstruct();
        };
    }

    public override float OnTick()
    {
        base.OnTick();
        
        _current -= 0.025f;
        progress.FillAnimated = _current;

        return _current;
    }

    public override void OnExplode()
    {
        base.OnExplode();
        
        Particle.Missile_Explosion.Play(base.Position);
        UIManagerExtensions.Distort(Position, 0.0f);

        OnDeconstruct();
    }

    private void StartTimer()
    {
        progress.gameObject.SetActive(true);
        _current = 1.0f;
        progress.Fill = _current;
    }
    private void StopTimer()
    {
        progress.Kill();
        progress.gameObject.SetActive(false);
    }

   
}
