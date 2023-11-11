
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
        
        // Tween upTween = ThisTransform.DOPunchPosition(new Vector3(0.0f, 0.5f, 0.0f), duration, 1);
        Tween shrinkTween = ThisTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

        // Sequence.Join(upTween);
        Sequence.Join(shrinkTween);

        Sequence.onComplete = () =>
        {
            OnDeconstruct();
        };
    }
}
