using DG.Tweening;
using Game;
using UnityEngine;

public class Nugget : SubModel
{
    public override void OnUnpack()
    {
        base.OnUnpack();
        
        RefreshSequence();

        const float duration = 0.25f;

        Tween rotTween = ThisTransform.DORotate(new Vector3(0.0f, 180.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine);
        Tween jumpTween = ThisTransform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        Sequence.Join(rotTween);
        Sequence.Join(jumpTween);
        
        Audio.Powerup_Throw.PlayOneShot();


        Sequence.onComplete = () =>
        {
            Audio.Powerup_Gold.PlayOneShot();

            UIManagerExtensions.BoardCoinToPlayer(Position,  10, 10);
            OnDeconstruct();
        };
    }
}
