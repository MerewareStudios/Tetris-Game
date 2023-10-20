using DG.Tweening;
using Game;
using UnityEngine;

public class Gift : SubModel
{
    public override bool OnCustomUnpack()
    {
        base.OnCustomUnpack();
        return false;
    }

    public override void OnAnimate(System.Action onComplete)
    {
        RefreshSequence();
        
        const float duration = 0.35f;
        
        Tween scaleTween = _transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        Tween rotateTween = _transform.DORotate(new Vector3(0.0f, 245.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetRelative(true).SetEase(Ease.OutSine);

        
        Sequence.Join(scaleTween);
        Sequence.Join(rotateTween);

        Sequence.onComplete = () =>
        {
            Particle.Confetti.Play(_transform.position, scale: new Vector3(3.3f, 3.3f, 3.3f), rotation: Quaternion.Euler(-90.0f, 0.0f, 0.0f));
            onComplete?.Invoke();
            OnDeconstruct();
        };
    }
}
