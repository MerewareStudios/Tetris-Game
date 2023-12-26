using DG.Tweening;
using UnityEngine;

public class Medic : SubModel
{
    public override void OnUnpack()
    {
        base.OnUnpack();
        
        RefreshSequence();

        const float duration = 0.25f;

        Tween scaleTween = ThisTransform.DOScale(Vector3.one * 0.5f, duration).SetRelative(true).SetEase(Ease.InBack);
        Tween jumpTween = ThisTransform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        Sequence.Join(scaleTween);
        Sequence.Join(jumpTween);
        
        Audio.Powerup_Throw.PlayOneShot();


        Sequence.onComplete = () =>
        {
            Audio.Heart.PlayOneShot();

            UIManagerExtensions.BoardHeartToPlayer(Position,  5, 5);
            OnDeconstruct();
        };
    }
}
