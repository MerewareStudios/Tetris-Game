using DG.Tweening;
using UnityEngine;

public class Landmine : SubModel
{
    public override void OnDeploy(Vector3 target, System.Action<SubModel> onComplete)
    {
        base.OnDeploy(target, onComplete);
        
        RefreshSequence();

        ThisTransform.localRotation = Quaternion.identity;
        
        float duration = Random.Range(0.45f, 0.65f);
        
        Tween moveTween = ThisTransform.DOMove(target, duration).SetEase(Ease.InOutSine);
        Tween rotTween = ThisTransform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.WorldAxisAdd).SetEase(Ease.Linear);
        Tween scaleTween = ThisTransform.DOScale(Vector3.one * 0.9f, duration).SetEase(Ease.InBack);

        Sequence.Join(moveTween);
        Sequence.Join(rotTween);
        Sequence.Join(scaleTween);
        
        Audio.Powerup_Throw.PlayOneShot();

        Sequence.onComplete = () =>
        {
            onComplete?.Invoke(this);
        };
    }
}
