using UnityEngine;
using UnityEngine.Events;

public class EnemyCast : MonoBehaviour
{
    [SerializeField] private UnityEvent castEvent;
    
    public void Cast()
    {
        castEvent?.Invoke();    
    }
}
