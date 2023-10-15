using System;
using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Shop : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform bigIconTransform;
    [SerializeField] private RectTransform leftPivot;
    [SerializeField] private RectTransform smallIconTransform;
    [SerializeField] private RectTransform buttonTransform;
    [SerializeField] private Animator animator;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Button button;
    [SerializeField] private Data data;
    [SerializeField] private Color emptyColor;
    [SerializeField] private Color filledColor;
    [SerializeField] private Image[] bars;
    [SerializeField] private GameObject barParent;
    [System.NonSerialized] private bool _open = false;

    public bool BarEnabled
    {
        set => barParent.SetActive(value);
    }
    public Data _Data
    {
        set
        {
            this.data = value;
            if (data.available)
            {
                ImmediateShow();
            }
        }
        get => this.data;
    }
    
    public int Current
    {
        get => _Data.current;
        set
        {
            _Data.current = value;
            for (int i = 0; i < _Data.max; i++)
            {
                Color targetColor = (i < Current) ? filledColor : emptyColor;
                if (bars[i].color != targetColor)
                {
                    bars[i].DOKill();
                    bars[i].DOColor(targetColor, 0.2f);
                }
            }
        }
    }

    public void Increase(int count)
    {
        if (this._open || _Data.IsEnough)
        {
            return;
        }

        Current += count;
        if (_Data.IsEnough)
        {
            AnimatedShow();
        }
    }
    
    public void OnClick_Open()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        this._open = false;
        
        Current = 0;
        button.targetGraphic.raycastTarget = false;
        buttonTransform.DOKill();
        buttonTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetUpdate(true);
        MenuNavigator.THIS.Open();
    }

    private void ImmediateShow()
    {
        buttonTransform.gameObject.SetActive(true);
        buttonTransform.localScale = Vector3.one;
        animator.enabled = true;
        
        button.targetGraphic.raycastTarget = true;
    }
    
    public void AnimatedShow()
    {
        this._open = true;

        trailRenderer.emitting = false;
        button.targetGraphic.raycastTarget = false;
        this.WaitForNull(Show);
        
        void Show()
        {
            _canvas.enabled = true;

            bigIconTransform.gameObject.SetActive(true);
            buttonTransform.gameObject.SetActive(false);
            
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

                ImmediateShow();
                
                buttonTransform.DOKill();
                buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 1).SetUpdate(true);
                
                // int rewardAmount = Random.Range(8, 15);
                // UIManagerExtensions.EmitLevelShopCoin(smallIconTransform.position, rewardAmount, rewardAmount);
            };
        }
    }

    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public bool available = false    ;
        [SerializeField] public int current = 0;
        [SerializeField] public int max = 6;
        
        public Data()
        {
                
        }
        public Data(Data data)
        {
            this.available = data.available;
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
