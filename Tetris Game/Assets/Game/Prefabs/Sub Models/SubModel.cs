using System;
using DG.Tweening;
using Game;
using UnityEngine;

public class SubModel : MonoBehaviour
{
    [System.NonSerialized] protected Transform _transform;
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] protected AnimType animType;
    [System.NonSerialized] protected Sequence Sequence = null;

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

    public virtual void OnConstruct(Transform customParent)
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();

        Tween mainTween = null;
        Tween jumpTween = null;

        _transform.parent = customParent;
        
        _transform.localRotation = Quaternion.identity;
        _transform.localPosition = Vector3.zero;
        _transform.localScale = Vector3.one;

        const float duration = 1.5f;
        
        switch (animType)
        {
            case AnimType.None:
                return;
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
            Sequence.Join(mainTween);
        }
        if (jumpTween != null)
        {
            Sequence.Join(jumpTween);
        }
        Sequence.SetLoops(-1);
        Sequence.AppendInterval(2.5f);
    }

    public void Lose()
    {
        _transform.parent = null;
        Board.THIS.LoseSubModels.Add(this);
    }
    public virtual void OnDeconstruct()
    {
        Sequence?.Kill();
        Board.THIS.LoseSubModels.Remove(this);
        this.Despawn();
    }
    public void DeconstructImmediate()
    {
        Sequence?.Kill();
        this.Despawn();
    }
    public virtual void OnUse(Vector3 target)
    {
        
    }
    public virtual void OnUse()
    {
        
    }
    public virtual void OnProjectile(Enemy enemy)
    {
        
    }
    public virtual void OnAnimate(System.Action onComplete)
    {
        
    }
    public virtual float OnTick()
    {
        return 1.0f;
    }
    public virtual void OnExplode()
    {
        
    }
    
    public virtual bool OnCustomUnpack()
    {
        return true;
    }

    public void Rise(System.Action<Vector3> onComplete, float rotation = 180.0f, float duration = 0.25f)
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();


        // const float duration = 0.35f;
        
        Tween rotTween = _transform.DORotate(new Vector3(0.0f, rotation, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InOutSine);
        Tween jumpTween = _transform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        Sequence.Join(rotTween);
        Sequence.Join(jumpTween);

        Sequence.onComplete = () =>
        {
            onComplete?.Invoke(_transform.position);
            OnDeconstruct();
        };
    }
    
    
    public void Scale(System.Action<Vector3> onComplete, float rotation = 180.0f, float duration = 0.25f)
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();


        Tween scaleTween = _transform.DOScale(Vector3.one * 0.5f, duration).SetRelative(true).SetEase(Ease.InBack);
        Tween jumpTween = _transform.DOMove(new Vector3(0.0f, 0.5f, 0.0f), duration).SetRelative(true).SetEase(Ease.InOutSine);

        Sequence.Join(scaleTween);
        Sequence.Join(jumpTween);

        Sequence.onComplete = () =>
        {
            onComplete?.Invoke(_transform.position);
            OnDeconstruct();
        };
    }
    
    public void Shrink()
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();


        const float duration = 0.25f;
        
        Tween shrinkTween = _transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack);

        Sequence.Join(shrinkTween);

        Sequence.onComplete = () =>
        {
            OnDeconstruct();
        };
    }
    
    public void Missile(Vector3 target)
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();


        Tween jumpTween = _transform.DOJump(target, AnimConst.THIS.missileJumpPower, 1, AnimConst.THIS.missileDuration).SetEase(AnimConst.THIS.missileEase, AnimConst.THIS.missileOvershoot);

        Vector3 lastPos = _transform.position;
        jumpTween.onUpdate = () =>
        {
            var current = _transform.position;
            Vector3 direction = (current - lastPos);
            _transform.up = Vector3.Lerp(_transform.up,  direction, Time.deltaTime * jumpTween.ElapsedPercentage() * 80.0f);
            lastPos = current;
        };

        Sequence.Join(jumpTween);

        Sequence.onComplete = () =>
        {
            Particle.Missile_Explosion.Play(target);
            Warzone.THIS.AEODamage(target, 10, 2.0f);
            OnDeconstruct();
        };
    }
    public void Land(Vector3 target)
    {
        Sequence?.Kill();
        _transform.DOKill();
        Sequence = DOTween.Sequence();

        _transform.localRotation = Quaternion.identity;
        
        const float duration = 0.5f;
        
        Tween moveTween = _transform.DOMove(target, duration).SetEase(Ease.InBack);
        Tween rotTween = _transform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.LocalAxisAdd).SetEase(Ease.InBack);
        Tween scaleTween = _transform.DOScale(Vector3.one * 0.75f, duration).SetEase(Ease.InBack);

        Sequence.Join(moveTween);
        Sequence.Join(rotTween);
        Sequence.Join(scaleTween);

        Sequence.onComplete = () =>
        {
            Warzone.THIS.AddLandMine(this);
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
