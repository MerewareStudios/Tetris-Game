using DG.Tweening;
using Game;
using UnityEngine;

public class Gift : SubModel
{
    [SerializeField] private ParticleSystem ps;
    
    public override bool OnCustomUnpack()
    {
        base.OnCustomUnpack();
        return false;
    }

    public override void OnAnimate(System.Action onComplete)
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();
        
        const float duration = 0.35f;
        
        Tween scaleTween = _transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

        Sequence.Join(scaleTween);

        Sequence.onComplete = () =>
        {
            ps.Play();
            onComplete?.Invoke();
            OnDeconstruct();
        };
    }
}
