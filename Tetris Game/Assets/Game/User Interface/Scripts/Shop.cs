using System;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Shop : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private RectTransform bigIconTransform;
    [SerializeField] private RectTransform leftPivot;
    [SerializeField] private RectTransform smallIconTransform;
    [SerializeField] private RectTransform buttonTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Button button;
    [SerializeField] private Data data;
    [System.NonSerialized] public bool Open = false;

    public Data _Data
    {
        set
        {
            this.data = value;
        }
        get => this.data;
    }

    public void Increase()
    {
        if (this.Open || _Data.IsEnough)
        {
            return;
        }

        _Data.current++;
        Debug.LogWarning("SHOP POINT : " + _Data.current + "/" + _Data.max);
        if (_Data.IsEnough)
        {
            AnimatedShow();
        }
    }
    
    public void OnClick_Open()
    {
        this.Open = false;
        
        _Data.current = 0;
        button.targetGraphic.raycastTarget = false;
        buttonTransform.DOKill();
        buttonTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetUpdate(true);
        
        MenuNavigator.THIS.Open();
    }
    
    public void AnimatedShow()
    {
        this.Open = true;

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
                
                buttonTransform.DOKill();
                buttonTransform.localScale = Vector3.one;


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
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public int current = 0;
        [SerializeField] public int max = 6;
            
        public Data()
        {
                
        }
        public Data(Data data)
        {
            this.current = data.current;
            this.max = data.max;
        }

        public bool IsEnough => current >= max;

        public object Clone()
        {
            return new Data(this);
        }
    }
}
