using DG.Tweening;
using Game;
using UnityEngine;
using Random = UnityEngine.Random;

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
        thisTransform.parent = null;
        thisTransform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack).onComplete = () =>
        {
            
            switch (this.type)
            {
                case Type.MaxStack:
                    Board.THIS.StackLimit++;
                    break;
                case Type.Health:
                    UIManagerExtensions.BoardHeartToPlayer(thisTransform.position,  10, 50);
                    break;
                case Type.Chest:
                    // UIManagerExtensions.EmitChestCoinBurst(thisTransform.position, 15, 50);
                    UIManagerExtensions.EmitChestGemBurst(thisTransform.position, 10, 10);
                    break;
                case Type.Intel:
                    Spawner.THIS.SetNextBlockVisibility(true);
                    break;
            }
            
            Particle.Confetti.Play(thisTransform.position, Quaternion.Euler(-90.0f, 0.0f, 0.0f), new Vector3(2.5f, 2.5f, 2.5f));
            this.Despawn(pool);
        };

        
    }
    
    [SerializeField]
    public enum Type
    {
        MaxStack,
        Health,
        Chest,
        Intel,
    }
}
