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

    public override void OnUse(Vector3 target)
    {
        base.OnUse(target);
        
        StopTimer();
        
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();


        Tween jumpTween = _transform.DOJump(target, 4.0f, 1, 0.75f).SetEase(Ease.InSine);
        Tween rotateTween = _transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), 0.75f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetRelative(true);

        Sequence.Join(jumpTween);
        Sequence.Join(rotateTween);

        Sequence.onComplete = () =>
        {
            Particle.Missile_Explosion.Play(target);
            Warzone.THIS.AEODamage(target, 15, 2.0f);
            CameraManager.THIS.Shake(0.2f, 0.5f);
            UIManagerExtensions.Distort(Position, 0.0f);
            OnDeconstruct();
        };
    }

    public override float OnTick()
    {
        base.OnTick();
        
        _current -= 0.075f;
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
