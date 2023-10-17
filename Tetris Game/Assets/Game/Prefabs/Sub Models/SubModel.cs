using System;
using DG.Tweening;
using Game;
using UnityEngine;

public class SubModel : MonoBehaviour
{
    [System.NonSerialized] private Transform _transform;
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] private AnimType animType;
    [System.NonSerialized] private Sequence _sequence = null;

    public Vector3 Position => _transform.position; 
    
    void Awake()
    {
        _transform = this.transform;
    }

    public Color BaseColor
    {
        set
        {
            if (meshRenderer)
            {
                meshRenderer.material.SetColor(GameManager.BaseColor, value);
            }
        }
    }

    public void OnConstruct(Transform p)
    {
        _sequence?.Kill();
        _transform.DOKill();
        _sequence = DOTween.Sequence();

        Tween mainTween = null;
        Tween jumpTween = null;

        _transform.parent = p;
        
        _transform.localRotation = Quaternion.identity;
        _transform.localPosition = Vector3.zero;
        _transform.localScale = Vector3.one;

        float duration = 1.5f;
        
        switch (animType)
        {
            case AnimType.None:

                break;
            case AnimType.LeftRightShake:
                mainTween = _transform.DOPunchRotation(new Vector3(0.0f, 0.0f, 10.0f), duration, 3).SetEase(Ease.InOutSine);
                break;
            case AnimType.Rotate:
                mainTween = _transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
                jumpTween = _transform.DOPunchPosition(new Vector3(0.0f, 0.2f, 0.0f), duration, 1).SetEase(Ease.InOutSine);
                break;
        }

        if (mainTween != null)
        {
            _sequence.Join(mainTween);
        }
        if (jumpTween != null)
        {
            _sequence.Join(jumpTween);
        }
        _sequence.SetLoops(-1);
        _sequence.AppendInterval(2.5f);
    }

    public void OnDeconstruct()
    {
        _sequence?.Kill();
        this.Despawn();
    }


    public void Rise(System.Action<Vector3> onComplete, float rotation = 180.0f, float duration = 0.25f)
    {
        _sequence?.Kill();
        _transform.DOKill();
        _sequence = DOTween.Sequence();


        // const float duration = 0.35f;
        
        Tween rotTween = _transform.DORotate(new Vector3(0.0f, rotation, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine);
        Tween jumpTween = _transform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        _sequence.Join(rotTween);
        _sequence.Join(jumpTween);

        _sequence.onComplete = () =>
        {
            onComplete?.Invoke(_transform.position);
            OnDeconstruct();
        };
    }
    
    
    public void Scale(System.Action<Vector3> onComplete, float rotation = 180.0f, float duration = 0.25f)
    {
        _sequence?.Kill();
        _transform.DOKill();
        _sequence = DOTween.Sequence();


        Tween scaleTween = _transform.DOScale(Vector3.one * 0.5f, duration).SetRelative(true).SetEase(Ease.InBack);
        Tween jumpTween = _transform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        _sequence.Join(scaleTween);
        _sequence.Join(jumpTween);

        _sequence.onComplete = () =>
        {
            onComplete?.Invoke(_transform.position);
            OnDeconstruct();
        };
    }
    
    // public void Lerp2Player(System.Action onComplete)
    // {
    //     _sequence?.Kill();
    //     _transform.DOKill();
    //     _sequence = DOTween.Sequence();
    //
    //     
    //     
    //     const float duration = 0.25f;
    //     
    //     Tween rotTween = _transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine);
    //     Tween jumpTween = _transform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);
    //
    //     _sequence.Join(rotTween);
    //     _sequence.Join(jumpTween);
    //     
    //     
    //
    //     // const float duration = 0.5f;
    //     
    //     // Tween moveUpTween = _transform.DOMove(new Vector3(0.0f, 1.5f, 0.0f), 0.25f).SetRelative(true).SetEase(Ease.Linear);
    //     // Tween rotTween = _transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.WorldAxisAdd).SetRelative(true).SetEase(Ease.InOutSine);
    //     // Tween moveTween = _transform.DOMove(Warzone.THIS.Player.lerpTarget.position, duration).SetEase(Ease.InBack);
    //     // Tween scaleTween = _transform.DOScale(Vector3.one * 0.5f, duration).SetEase(Ease.InBack);
    //     //
    //     // _sequence.Append(moveUpTween);
    //     // _sequence.Join(rotTween);
    //     // _sequence.Append(moveTween);
    //     // _sequence.Join(scaleTween);
    //
    //     _sequence.onComplete = () =>
    //     {
    //         Particle.EnergyExplosionYellow.Play(transform.position);
    //
    //         OnDeconstruct();
    //         onComplete?.Invoke();
    //     };
    // }
    
    public void Shrink()
    {
        _sequence?.Kill();
        _transform.DOKill();
        _sequence = DOTween.Sequence();


        const float duration = 0.25f;
        
        Tween shrinkTween = _transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

        _sequence.Join(shrinkTween);

        _sequence.onComplete = () =>
        {
            OnDeconstruct();
        };
    }
    
    public void Missile(Vector3 target)
    {
        _sequence?.Kill();
        _transform.DOKill();
        _sequence = DOTween.Sequence();


        Tween jumpTween = _transform.DOJump(target, AnimConst.THIS.missileJumpPower, 1, AnimConst.THIS.missileDuration).SetEase(AnimConst.THIS.missileEase, AnimConst.THIS.missileOvershoot);

        Vector3 lastPos = _transform.position;
        jumpTween.onUpdate = () =>
        {
            var current = _transform.position;
            Vector3 direction = (current - lastPos);
            _transform.up = Vector3.Lerp(_transform.up,  direction, Time.deltaTime * jumpTween.ElapsedPercentage() * 80.0f);
            lastPos = current;
        };

        _sequence.Join(jumpTween);

        _sequence.onComplete = () =>
        {
            Particle.Missile_Explosion.Play(target);
            Warzone.THIS.AEODamage(target, 10, 2.0f);
            OnDeconstruct();
        };
    }
    public void Land(Vector3 target)
    {
        _sequence?.Kill();
        _transform.DOKill();
        _sequence = DOTween.Sequence();

        _transform.localRotation = Quaternion.identity;
        
        const float duration = 0.5f;
        
        Tween moveTween = _transform.DOMove(target, duration).SetEase(Ease.InBack);
        Tween rotTween = _transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InBack);
        Tween scaleTween = _transform.DOScale(Vector3.one * 0.75f, duration).SetEase(Ease.InBack);

        _sequence.Join(moveTween);
        _sequence.Join(rotTween);
        _sequence.Join(scaleTween);

        _sequence.onComplete = () =>
        {
            Warzone.THIS.AddLandMine(this);
            // OnDeconstruct();
        };
    }

    [Serializable]
    public enum AnimType
    {
        None,
        LeftRightShake,
        Rotate,
    }
}
