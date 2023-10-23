using System;
using DG.Tweening;
using Game;
using UnityEngine;

public class SubModel : MonoBehaviour
{
    [System.NonSerialized] protected Transform ThisTransform;
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] protected AnimType animType;
    [System.NonSerialized] protected Sequence Sequence = null;
    [System.NonSerialized] protected int ExternalValue = 0;

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
                meshRenderer.material.SetColor(GameManager.BaseColor, value);
            }
        }
    }

    public virtual void OnConstruct(Transform customParent, int extra)
    {
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
        ThisTransform.DOKill();
        Board.THIS.LoseSubModels.Remove(this);
        this.Despawn();
    }
    public void DeconstructImmediate()
    {
        Sequence?.Kill();
        this.Despawn();
    }
    
    public virtual bool OnCustomUnpack()
    {
        return true;
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
        
    }
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
