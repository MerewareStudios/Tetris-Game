using UnityEngine;
using UnityEngine.Events;

public class EnemyCast : MonoBehaviour
{
    [SerializeField] private UnityEvent castEvent;
    [SerializeField] private UnityEvent canWalkEvent;
    
    public void Cast()
    {
        castEvent?.Invoke();    
    }
    public void CanWalk()
    {
        canWalkEvent?.Invoke();    
    }
}
