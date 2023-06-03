using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

public class FlyingText : Singleton<FlyingText>
{
    [SerializeField] public Canvas canvas;
    [SerializeField] public Camera worldCamera;
    public delegate TextMeshProUGUI GetInstance();
    public System.Action<MonoBehaviour> ReturnInstance;
    public GetInstance OnGetInstance;
    
    public void FlyWorld(string str, Vector3 worldPosition, float delay = 0.0f)
    {
        TextMeshProUGUI text = OnGetInstance.Invoke();
        text.text = str;
        
        RectTransform rectTransform = text.rectTransform;
        rectTransform.SetParent(this.transform);
        rectTransform.position = canvas.worldCamera.ScreenToWorldPoint(worldCamera.WorldToScreenPoint(worldPosition));
        
        rectTransform.DOKill();
        rectTransform.localScale = Vector3.zero;

        text.color = Color.white;

        float scaleUpDuration = 0.175f;
        float fadeDuration = 0.25f;
        
        Tween scaleTween = rectTransform.DOScale(Vector3.one, scaleUpDuration).SetEase(Ease.Linear).SetDelay(delay);
        Tween upTween = rectTransform.DOMove(Vector3.up * 0.625f, fadeDuration).SetRelative(true).SetEase(Ease.OutSine).SetDelay(0.0f);
        Tween fadeTween = text.DOFade(0.0f, fadeDuration).SetEase(Ease.OutQuint).SetDelay(0.175f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(scaleTween);
        sequence.Append(upTween);
        sequence.Join(fadeTween);

        sequence.onComplete += () => ReturnInstance.Invoke(text);
    }
}