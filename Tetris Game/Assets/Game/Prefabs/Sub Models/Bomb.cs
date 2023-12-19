using DG.Tweening;
using Game;
using UnityEngine;

public class Bomb : SubModel
{
    [SerializeField] private CircularProgress progress;
    [System.NonSerialized] private float _current;
    [System.NonSerialized] private float _tickInterval;

    public override void OnConstruct(Pool poolType, Transform customParent, int extra)
    {
        base.OnConstruct(poolType, customParent, extra);
        _tickInterval = extra * 0.001f;
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

        Vector3 hitTarget = enemy.hitTarget.position;


        Tween jumpTween = ThisTransform.DOJump(hitTarget, 4.0f, 1, 0.75f).SetEase(Ease.InSine);
        Tween rotateTween = ThisTransform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), 0.75f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetRelative(true);

        Sequence.Join(jumpTween);
        Sequence.Join(rotateTween);

        Sequence.onComplete = () =>
        {
            Warzone.THIS.AEODamage(hitTarget, 15, 2.0f);
            
            Particle.Missile_Explosion.Play(hitTarget);
            Audio.Bomb_Explode.PlayOneShot();
            CameraManager.THIS.Shake(0.25f, 0.5f);
            UIManagerExtensions.QuickDistort(Position);
            
            OnDeconstruct();
        };
    }

    public override float OnTick()
    {
        base.OnTick();
        _current -= _tickInterval;
        progress.FillAnimated = _current;
        return _current;
    }

    public override void OnExplode()
    {
        base.OnExplode();
        
        Particle.Missile_Explosion.Play(base.Position);
        Audio.Bomb_Explode.PlayOneShot();

        UIManagerExtensions.Distort(Position, 9.0f, 0.05f, 1.1f, Ease.OutSine);

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
