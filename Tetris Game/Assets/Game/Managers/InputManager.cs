using Internal.Core;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : Singleton<InputManager>
{
    [System.NonSerialized] private const float TapInterval = 0.25f;
    [SerializeField] private float moveThreshold = 10f; 
    [System.NonSerialized] private float _touchBegin; 
#if UNITY_EDITOR
    [System.NonSerialized] private Vector3 beginPosition; 
#else
    [System.NonSerialized] private Vector2 beginPosition; 
    [System.NonSerialized] private bool Moving = false; 
#endif
    [Header("Events")] 
    [SerializeField] private UnityEvent<Vector3> OnDown; 
    [SerializeField] private UnityEvent<Vector3> OnTap; 
    [SerializeField] private UnityEvent<Vector3> OnMove; 
    [SerializeField] private UnityEvent OnRelease;

#if UNITY_EDITOR
     private void Update()
     {
         if (Input.GetMouseButtonDown(0))
         {
             beginPosition = Input.mousePosition;
             _touchBegin = Time.time;
             OnDown?.Invoke(Input.mousePosition);
         }
         else if (Input.GetMouseButtonUp(0))
         {
             if (Time.time - _touchBegin <= TapInterval)
             {
                 OnTap?.Invoke(Input.mousePosition);
             }
             OnRelease?.Invoke();
         }
         else
         {
             if ((beginPosition - Input.mousePosition).magnitude > moveThreshold)
             {
                 OnMove?.Invoke(Input.mousePosition);
             }
         }
     }
#else
    private void Update()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }
        
        Touch touch = Input.GetTouch(0);
    
        switch (touch.phase)
        {
            case TouchPhase.Began:
                beginPosition = touch.position;
                _touchBegin = Time.time;
                OnDown?.Invoke(touch.position);
                break;
            case TouchPhase.Ended:
            {
                if (Time.time - _touchBegin <= TapInterval && !Moving)
                {
                    OnTap?.Invoke(touch.position);
                }
                OnRelease?.Invoke();
                Moving = false;
                break;
            }
            case TouchPhase.Moved:
            {
                if (Moving || (beginPosition - touch.position).magnitude > moveThreshold)
                {
                    Moving = true;
                    OnMove?.Invoke(touch.position);
                }
    
                break;
            }
        }
    }
#endif
    

}
