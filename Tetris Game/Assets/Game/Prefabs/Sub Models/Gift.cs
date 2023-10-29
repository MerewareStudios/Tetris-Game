using DG.Tweening;
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
        
        Tween scaleTween = ThisTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        Tween rotateTween = ThisTransform.DORotate(new Vector3(0.0f, 245.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetRelative(true).SetEase(Ease.OutSine);

        
        Sequence.Join(scaleTween);
        Sequence.Join(rotateTween);

        Sequence.onComplete = () =>
        {
            Particle.Confetti.Play(ThisTransform.position, Quaternion.Euler(-90.0f, 0.0f, 0.0f), new Vector3(3.3f, 3.3f, 3.3f));
            onComplete?.Invoke();
            OnDeconstruct();
        };
    }
}
