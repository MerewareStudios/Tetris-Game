
using DG.Tweening;
using UnityEngine;

public class Magnet : SubModel
{
    public override void OnUse()
    {
        base.OnUse();
        
        RefreshSequence();
        
        UIManagerExtensions.Distort(Position, 0.0f);

        const float duration = 0.25f;
        
        Tween shrinkTween = ThisTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

        Sequence.Join(shrinkTween);

        Sequence.onComplete = () =>
        {
            OnDeconstruct();
        };
    }
}
