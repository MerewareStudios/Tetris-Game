using DG.Tweening;
using Game;
using UnityEngine;

public class Energy : SubModel
{
    public override void OnUse()
    {
        base.OnUse();
        
        RefreshSequence();

        const float duration = 0.25f;

        Tween rotTween = ThisTransform.DORotate(new Vector3(0.0f, 180.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine);
        Tween jumpTween = ThisTransform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        Sequence.Join(rotTween);
        Sequence.Join(jumpTween);

        Sequence.onComplete = () =>
        {
            Warzone.THIS.Player.Gun.Boost();
            Particle.Energy.Play(Position);
            UIManagerExtensions.Distort(Position, 0.0f);
            OnDeconstruct();
        };
    }
}
