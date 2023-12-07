using DG.Tweening;
using Internal.Core;
using UnityEngine;

namespace Game.UI
{
    public enum MenuType
    {
        Block,
        Weapon,
    }
    public class Menu<T> : Lazyingleton<T>, IMenu where T : MonoBehaviour
    {
        [SerializeField] private RectTransform parentContainer;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private bool updateOnMoneyChange = true;
        [System.NonSerialized] private Tween _showTween;
        [System.NonSerialized] public bool Visible = false;
        [field: System.NonSerialized] public int TotalNotify { get; set; }

        
        
        public bool Open(float duration = 0.5f)
        {
            if (Visible)
            {
                return true;
            }
            this.gameObject.SetActive(true);
            Visible = true;
            canvas.enabled = true;
            canvasGroup.alpha = 0.0f;
            _showTween?.Kill();
            _showTween = canvasGroup.DOFade(1.0f, duration).SetEase(Ease.InOutSine).SetUpdate(true);

            if (updateOnMoneyChange)
            {
                UIManager.CurrentMenu = this;
            }
            
            return false;
        }
        
        public bool Close(float duration = 0.25f, float delay = 0.0f)
        {
            if (!Visible)
            {
                return true;
            }
            Visible = false;
            
            _showTween?.Kill();
            _showTween = canvasGroup.DOFade(0.0f, duration).SetEase(Ease.InOutSine).SetDelay(delay).SetUpdate(true);
            _showTween.onComplete = () =>
            {
                canvas.enabled = false;
                this.gameObject.SetActive(false);
            };

            if (updateOnMoneyChange)
            {
                UIManager.CurrentMenu = null;
            }            
            return false;
        }
        public void Show()
        {
            
        }
        public RectTransform GetParentContainer()
        {
            return this.parentContainer;
        }

        int IMenu.AvailablePurchaseCount(bool updatePage)
        {
            return 0;
        }


        public void CloseImmediate()
        {
            if (!Visible)
            {
                return;
            }
            Visible = false;
            canvas.enabled = false;  
        }
    }

    

    public interface IMenu
    {
        bool Open(float duration = 0.25f);
        bool Close(float duration = 0.5f, float delay = 0.0f);
        void Show();
        RectTransform GetParentContainer();
        int AvailablePurchaseCount(bool updatePage);
        int TotalNotify { get; set; }
    }
}
