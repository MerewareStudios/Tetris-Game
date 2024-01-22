using System;
using DG.Tweening;
using Game;
using UnityEngine;

public class SubModel : MonoBehaviour
{
    [System.NonSerialized] public Pool _poolType;
    [System.NonSerialized] protected Transform ThisTransform;
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] protected AnimType animType;
    [System.NonSerialized] protected Sequence Sequence = null;
    [System.NonSerialized] protected int ExternalValue = 0;
    [System.NonSerialized] private Tween _delayedTween = null;
    [System.NonSerialized] protected Color _color;

    public Vector3 Position => ThisTransform.position; 
    
    void Awake()
    {
        ThisTransform = this.transform;
    }

    public Color BaseColor
    {
        set
        {
            if (meshRenderer)
            {
                _color = value;
                meshRenderer.material.SetColor(GameManager.BaseColor, value);
            }
        }
    }

    public void ResetEmission()
    {
        meshRenderer.material.SetColor(GameManager.EmissionKey, Color.black);
    }
    public void OnMerge()
    {
        meshRenderer.material.DOGradientColor(Const.THIS.mergeGradient, "_EmissionColor", 0.25f).SetEase(Ease.Linear);
    }
    public virtual void EmitExplodeEffect()
    {
        Particle.Debris.Emit(15, Position, Color.black);
    }
    public virtual bool IsAvailable()
    {
        return true;
    }
    public virtual void MarkAvailable(bool state)
    {
        
    }
    public virtual void MakeAvailable()
    {
        ThisTransform.DOKill();
        ThisTransform.localScale = Vector3.zero;
            
        _delayedTween?.Kill();
        _delayedTween = DOVirtual.DelayedCall(AnimConst.THIS.MergeShowDelay, () =>
        {
            UIManagerExtensions.QuickDistort(ThisTransform.position + Vector3.up * 0.45f);
            Particle.Merge_Circle.Play(ThisTransform.position  + new Vector3(0.0f, 0.85f, 0.0f), Quaternion.identity, Vector3.one * 0.5f);
        
            ThisTransform.DOKill();
            ThisTransform.localScale = Vector3.one;
            ThisTransform.DOPunchScale(Vector3.one * AnimConst.THIS.mergedScalePunch, AnimConst.THIS.mergedScaleDuration, 1).onComplete = () =>
            {
                MarkAvailable(true);
            };
        }, false);
    }
    public virtual void OnConstruct(Pool poolType, Transform customParent, int extra)
    {
        this._poolType = poolType;
        OnExtraValueChanged(extra);
        
        RefreshSequence();

        Tween mainTween = null;
        Tween jumpTween = null;

        ThisTransform.parent = customParent;
        
        ThisTransform.localRotation = Quaternion.identity;
        ThisTransform.localPosition = Vector3.zero;
        ThisTransform.localScale = Vector3.one;

        const float duration = 1.5f;
        
        switch (animType)
        {
            case AnimType.None:
                return;
            case AnimType.LeftRightShake:
                mainTween = ThisTransform.DOPunchRotation(new Vector3(0.0f, 0.0f, 10.0f), duration, 3).SetEase(Ease.InOutSine);
                break;
            case AnimType.Rotate:
                mainTween = ThisTransform.DORotate(new Vector3(0.0f, 360.0f, 0.0f), duration, RotateMode.FastBeyond360).SetEase(Ease.InOutSine);
                jumpTween = ThisTransform.DOPunchPosition(new Vector3(0.0f, 0.2f, 0.0f), duration, 1).SetEase(Ease.InOutSine);
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

    protected void RefreshSequence()
    {
        Sequence?.Kill();
        ThisTransform.DOKill();
        Sequence = DOTween.Sequence();
    }

    public void Lose()
    {
        ThisTransform.parent = null;
        Board.THIS.LoseSubModels.Add(this);
    }
    public void UnParent()
    {
        ThisTransform.parent = null;
    }
    public virtual void OnDeconstruct()
    {
        Sequence?.Kill();
        _delayedTween?.Kill();
        ThisTransform.DOKill();
        Board.THIS.LoseSubModels.Remove(this);
        OnDespawn();
    }
    public virtual void DeconstructImmediate()
    {
        Sequence?.Kill();
        _delayedTween?.Kill();
        OnDespawn();
    }

    public void OnDespawn()
    {
        this.Despawn(_poolType);
    }
    
    public virtual bool OnCustomUnpack()
    {
        return true;
    }
    public virtual void OnUnpack()
    {
        
    }
    public virtual void OnUse()
    {
        
    }
    public virtual int GetExtra()
    {
        return 0;
    }
    public virtual void SetExtra(int amount)
    {
        
    }
    public virtual void OnProjectile(Enemy enemy)
    {
        
    }
    public virtual void OnDeploy(Vector3 target, System.Action<SubModel> onComplete)
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
        EmitExplodeEffect();
    }
    // public virtual bool SkipAction()
    // {
    //     return false;
    // }
    public virtual void OnExtraValueChanged(int value)
    {
        this.ExternalValue = value;
    }

    [Serializable]
    public enum AnimType
    {
        None,
        LeftRightShake,
        Rotate,
    }
}
