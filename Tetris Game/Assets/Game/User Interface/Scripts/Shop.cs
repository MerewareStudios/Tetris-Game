using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private RectTransform bigIconTransform;
    [SerializeField] private RectTransform leftPivot;
    [SerializeField] private RectTransform smallIconTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Button button;
    
    public void AnimatedShow()
    {
        trailRenderer.emitting = false;
        button.targetGraphic.raycastTarget = false;
        this.WaitForNull(Show);
        
        void Show()
        {
            _canvas.enabled = true;

            bigIconTransform.gameObject.SetActive(true);
            pivot.gameObject.SetActive(false);
            
            trailRenderer.transform.SetParent(bigIconTransform);
            trailRenderer.transform.localPosition = Vector3.zero;


            bigIconTransform.DOKill();
            bigIconTransform.position = leftPivot.position;
            bigIconTransform.localScale = Vector3.one;
            Tween moveInside = bigIconTransform.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutQuad);
            Tween moveTarget = bigIconTransform.DOMove(smallIconTransform.position, 0.5f).SetEase(Ease.InBack);
            Tween scaleDown = bigIconTransform.DOScale(Vector3.one * 0.5f, 0.5f).SetEase(Ease.InBack);

            trailRenderer.Clear();
            trailRenderer.emitting = true;

            Sequence sequence = DOTween.Sequence();

            sequence.Append(moveInside).Append(moveTarget).Join(scaleDown);

            sequence.SetUpdate(true);
            
            
            sequence.onComplete  = () =>
            {
                trailRenderer.transform.SetParent(_canvas.transform);

                trailRenderer.emitting = false;

                bigIconTransform.gameObject.SetActive(false);
                pivot.gameObject.SetActive(true);
                pivot.localScale = Vector3.one;
                pivot.DOPunchScale(Vector3.one * 0.2f, 0.25f, 1).SetUpdate(true);

                animator.enabled = true;

                int rewardAmount = Random.Range(8, 15);
                button.targetGraphic.raycastTarget = true;

                UIManagerExtensions.EmitLevelShopCoin(smallIconTransform.position, rewardAmount, rewardAmount);
            };
        }
    }

    public void ImmediateShow()
    {
        
    }

    public void AnimatedHide()
    {
        
    }
    
    public void ImmediateHide()
    {
        
    }
}
