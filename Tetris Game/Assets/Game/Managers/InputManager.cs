using Internal.Core;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : Singleton<InputManager>
{
    [SerializeField] private float tapInterval = 0.1f; 
    [SerializeField] private float moveThreshold = 10f; 
    [System.NonSerialized] private float touchBegin; 
    [System.NonSerialized] private Vector2 beginPosition; 
    [System.NonSerialized] private bool Moving = false; 
    [Header("Events")] 
    [SerializeField] private UnityEvent<Vector3> OnDown; 
    [SerializeField] private UnityEvent<Vector3> OnTap; 
    [SerializeField] private UnityEvent<Vector3> OnMove; 
    [SerializeField] private UnityEvent OnRelease;

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                beginPosition = touch.position;
                touchBegin = Time.time;
                OnDown?.Invoke(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (Time.time - touchBegin <= tapInterval && !Moving)
                {
                    OnTap?.Invoke(touch.position);
                }
                OnRelease?.Invoke();
                Moving = false;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (Moving || (beginPosition - touch.position).magnitude > moveThreshold)
                {
                    Moving = true;
                    OnMove?.Invoke(touch.position);
                }
            }
        }
    }

}
