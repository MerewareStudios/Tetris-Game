using DG.Tweening;
using Game;
using UnityEngine;

public class Gift : SubModel
{
    public override void OnConstruct(Transform customParent)
    {
        base.OnConstruct(customParent);
    }

    public override bool OnCustomUnpack()
    {
        base.OnCustomUnpack();
        return false;
    }

    public override void OnUse()
    {
        base.OnUse();
        
        const float duration = 0.35f;
        
        Tween moveTween = _transform.DOMove(new Vector3(0.0f, 1.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.OutSine);
        Tween scaleTween = _transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

        Sequence.Append(moveTween);
        Sequence.Join(scaleTween);

        Sequence.onComplete = () =>
        {
            OnDeconstruct();
        };
        
    }

    private void Unscrew()
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();

        const float duration = 0.5f;

        
        Tween moveTween = _transform.DOMove(new Vector3(0.0f, 0.15f, 0.0f), duration).SetRelative(true).SetEase(Ease.OutSine);
        Tween rotateTween = _transform.DORotate(new Vector3(0.0f, 270.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetRelative(true).SetEase(Ease.OutSine);

        Sequence.Join(moveTween);
        Sequence.Join(rotateTween);
    }
}
