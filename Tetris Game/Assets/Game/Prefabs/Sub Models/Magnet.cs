using DG.Tweening;
using Game;
using UnityEngine;

public class Magnet : SubModel
{
    public override void OnPlace(Place place)
    {
        base.OnPlace(place);
        
        Board.THIS.AddMagneticPlace(place);
    }

    public override void OnUnpack()
    {
        base.OnUnpack();
        
        RefreshSequence();
        
        UIManagerExtensions.QuickDistort(Position);

        const float duration = 0.25f;
        
        // Tween upTween = ThisTransform.DOPunchPosition(new Vector3(0.0f, 0.5f, 0.0f), duration, 1);
        Tween shrinkTween = ThisTransform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);
        Tween rotateTween = ThisTransform.DOLocalRotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.FastBeyond360).SetEase(Ease.InBack);

        // Sequence.Join(upTween);
        Sequence.Join(shrinkTween);
        Sequence.Join(rotateTween);
        
        Audio.Magnet.PlayOneShot();
        Audio.Powerup_Throw.PlayOneShot();

        Sequence.onComplete = () =>
        {
            OnDeconstruct();
        };
    }
}
