using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class InputManager : Singleton<InputManager>
{
    [SerializeField] private float tapInterval = 0.1f; 
    [System.NonSerialized] private float touchBegin; 
    [Header("Events")] 
    [SerializeField] private UnityEvent<Vector3> OnTap; 
    [SerializeField] private UnityEvent<Vector3> OnMove; 
    [SerializeField] private UnityEvent<Vector3> OnRelease; 
    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchBegin = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                if (Time.time - touchBegin <= tapInterval)
                {
                    OnTap?.Invoke(touch.position);
                }
                OnRelease?.Invoke(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                OnMove?.Invoke(touch.position);
            }
        }
    }

}
