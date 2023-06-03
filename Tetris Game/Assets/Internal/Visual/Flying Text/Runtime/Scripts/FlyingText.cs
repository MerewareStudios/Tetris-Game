using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

public class FlyingText : Singleton<FlyingText>
{
    [System.NonSerialized] public Canvas canvas;
    public delegate TextMeshProUGUI GetInstance();
    public GetInstance GetInstanceAction;
    
    public void ShowWorld(Vector3 worldPosition)
    {
        TextMeshProUGUI text = GetInstanceAction.Invoke();
        text.transform.position = canvas.worldCamera.WorldToScreenPoint(worldPosition);
        
        text.transform.DOKill();
        text.transform.localScale = Vector3.zero;
        text.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack);
    }
}
public static class FlyingTextExtensions
{
    public static void Fly(this string value, Vector3 worldPosition)
    {
        FlyingText.THIS.ShowWorld(worldPosition);
    }
    public static void Fly(this int value, Vector3 worldPosition)
    {
        FlyingText.THIS.ShowWorld(worldPosition);
    }
}