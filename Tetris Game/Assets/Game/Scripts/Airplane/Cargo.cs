using DG.Tweening;
using UnityEngine;

public class Cargo : MonoBehaviour
{
    [SerializeField] public Transform thisTransform;
    [SerializeField] public Pool pool;
    [SerializeField] public Type type;
    [System.NonSerialized] private Tweener _dropTween;

    public void Place(Transform cargoParent)
    {
        int childCount = cargoParent.childCount;
        thisTransform.parent = cargoParent;
        
        thisTransform.localScale = Vector3.zero;
        thisTransform.localRotation = Quaternion.Euler(0.0f, Random.Range(-12.0f, 12.0f), 0.0f);
        thisTransform.localPosition = new Vector3(0.0f, 0.594f * childCount, 0.0f);

        thisTransform.DOKill();
        thisTransform.DOScale(Vector3.one, 0.25f).SetDelay(childCount * 0.1f).SetEase(Ease.OutBack);
    }
    public void Drop(Transform cargoParent, float altitude, System.Action onComplete)
    {
        int childCount = cargoParent.childCount;
        thisTransform.parent = cargoParent;
        
        thisTransform.localScale = Vector3.zero;
        thisTransform.localRotation = Quaternion.Euler(0.0f, Random.Range(-12.0f, 12.0f), 0.0f);
        thisTransform.localPosition = new Vector3(0.0f, altitude, 0.0f);

        thisTransform.DOKill();
        thisTransform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        _dropTween = thisTransform.DOLocalMove(new Vector3(0.0f, 0.594f * childCount, 0.0f), 2.1f);
        _dropTween
            .SetDelay(0.3f)
            .SetSpeedBased(true)
            .SetEase(Ease.OutBounce)
            .onComplete = onComplete.Invoke;
    }
    public void Redrop()
    {
        int childCount = thisTransform.parent.childCount - 1;
        _dropTween.ChangeEndValue(new Vector3(0.0f, 0.594f * childCount, 0.0f), true);
    }

    public void Unpack()
    {
        transform.parent = null;
        thisTransform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InBack).onComplete = () =>
        {
            this.Despawn(pool);
        };
    }
    
    [SerializeField]
    public enum Type
    {
        MaxStack,
        Health,
    }
}
