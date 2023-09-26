using DG.Tweening;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public enum MenuType
    {
        Block,
        Weapon,
        Upgrade
    }
    public class Menu<T> : Lazyingleton<T>, IMenu where T : MonoBehaviour
    {
        [SerializeField] private RectTransform parentContainer;
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Image _blocker;
        [SerializeField] private bool updateOnMoneyChange = true;
        [System.NonSerialized] private Tween showTween;
        [System.NonSerialized] protected bool Visible = false;

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
            _blocker.raycastTarget = true;
            showTween?.Kill();
            showTween = canvasGroup.DoFade_IWI(1.0f, duration, Ease.InOutSine, () =>
            {
                _blocker.raycastTarget = false;
            }).SetUpdate(true);

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
            
            _blocker.raycastTarget = true;
            showTween?.Kill();
            showTween = canvasGroup.DoFade_IWI(0.0f, duration, Ease.InOutSine, () =>
            {
                canvas.enabled = false; 
                this.gameObject.SetActive(false);

            }).SetDelay(delay).SetUpdate(true);

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
        public void Show();
        RectTransform GetParentContainer();
    }
}
