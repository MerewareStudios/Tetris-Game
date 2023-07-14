using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class SpawnerInput : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler, IPointerClickHandler
{
    [SerializeField] private UnityEvent OnFingerDown;
    [SerializeField] private UnityEvent OnFingerUp;
    [SerializeField] private UnityEvent OnFingerClick;
    [SerializeField] private UnityEvent OnFingerDrag;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        OnFingerDown.Invoke();
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        OnFingerUp.Invoke();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnFingerClick.Invoke();
    }
    public void OnPointerMove(PointerEventData eventData)
    {
        OnFingerDrag.Invoke();
    }
}
