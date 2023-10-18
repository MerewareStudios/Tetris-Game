using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game;
using UnityEngine;

public class Bomb : SubModel
{
    [SerializeField] private CircularProgress progress;
    [System.NonSerialized] private Tween _timerTween;

    public override void OnConstruct(Transform p)
    {
        base.OnConstruct(p);
        
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
            Warzone.THIS.AEODamage(target, 10, 2.0f);
            OnDeconstruct();
        };
    }
    
    private void StartTimer()
    {
        progress.gameObject.SetActive(true);
        float step = 0.0f;
        _timerTween?.Kill();
        _timerTween = DOTween.To(x => step = x, 1.0f, 0.0f, 30.0f).SetEase(Ease.OutSine);
        _timerTween.onUpdate = () =>
        {
            progress.Fill = step;
        };
        _timerTween.onComplete = () =>
        {
            Particle.Missile_Explosion.Play(base.Position);
            OnDeconstruct();
        };
    }
    private void StopTimer()
    {
        progress.gameObject.SetActive(false);
        _timerTween?.Kill();
    }

   
}
