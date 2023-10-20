using DG.Tweening;
using UnityEngine;

public class Screw : SubModel
{
    [System.NonSerialized] private int _current = 3;

    public override void OnConstruct(Transform customParent)
    {
        base.OnConstruct(customParent);
        _current = 3;
    }

    public override bool OnCustomUnpack()
    {
        base.OnCustomUnpack();
        _current--;
        Unscrew();
        return _current <= 0;
    }

    public override void OnUse()
    {
        base.OnUse();
        
        const float duration = 0.35f;
        
        Tween moveTween = ThisTransform.DOMove(new Vector3(0.0f, 1.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.OutSine);
        Tween scaleTween = ThisTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

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
        ThisTransform.DOKill();
        Sequence = DOTween.Sequence();

        const float duration = 0.5f;

        
        Tween moveTween = ThisTransform.DOMove(new Vector3(0.0f, 0.15f, 0.0f), duration).SetRelative(true).SetEase(Ease.OutSine);
        Tween rotateTween = ThisTransform.DORotate(new Vector3(0.0f, 270.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetRelative(true).SetEase(Ease.OutSine);

        Sequence.Join(moveTween);
        Sequence.Join(rotateTween);
    }
}
