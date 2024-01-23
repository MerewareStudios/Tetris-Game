using DG.Tweening;
using Game;
using UnityEngine;

public class Bomb : SubModel
{
    [SerializeField] private CircularProgress progress;
    [System.NonSerialized] private int _duration;
    [System.NonSerialized] private Tween _timerTween;

    public override void OnConstruct(Pool poolType, Transform customParent, int extra)
    {
        base.OnConstruct(poolType, customParent, extra);
        _duration = extra;
        progress.gameObject.SetActive(false);
    }

    public override void OnDeconstruct()
    {
        base.OnDeconstruct();
        StopTimer();
    }

    public override void OnPlace(Place place)
    {
        base.OnPlace(place);
        StartTimer(_duration, place);
    }

    public override void OnProjectile(Enemy enemy)
    {
        base.OnProjectile(enemy);
        
        StopTimer();
        RefreshSequence();
        
        
        // if (!enemy)
        // {
        //     return;
        // }

        Vector3 hitTarget = enemy ? enemy.hitTarget.position : Warzone.THIS.RandomInvalidPosition();


        Tween jumpTween = ThisTransform.DOJump(hitTarget, 4.0f, 1, 0.75f).SetEase(Ease.InSine);
        Tween rotateTween = ThisTransform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), 0.75f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetRelative(true);

        Sequence.Join(jumpTween);
        Sequence.Join(rotateTween);
        
        Audio.Powerup_Throw.PlayOneShot();

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

    public override void OnExplode(Vector2Int index)
    {
        base.OnExplode(index);
        
        Particle.Missile_Explosion.Play(base.Position);
        Audio.Bomb_Explode.PlayOneShot();

        UIManagerExtensions.Distort(Position, 9.0f, 0.05f, 1.1f, Ease.OutSine);

        
        Board.THIS.ExplodePawnsCircular(index);

        // OnDeconstruct();
    }

    private void StartTimer(int time, Place place)
    {
        progress.gameObject.SetActive(true);
        
        float timeStep = 1.0f;
        _timerTween = DOTween.To(x => timeStep = x, 1.0f, 0.0f, time).SetEase(Ease.Linear).SetUpdate(false);
        _timerTween.onUpdate = () =>
        {
            progress.Fill = timeStep;
        };
        _timerTween.onComplete = () => Board.THIS.ExplodePawnsCircular(place.Index);
    }
    private void StopTimer()
    {
        progress.Kill();
        _timerTween?.Kill();
        progress.gameObject.SetActive(false);
    }

   
}
