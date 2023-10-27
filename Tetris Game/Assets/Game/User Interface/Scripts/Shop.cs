using DG.Tweening;
using Internal.Core;
using IWI.Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform bigIconTransform;
    [SerializeField] private RectTransform leftPivot;
    [SerializeField] private RectTransform smallIconTransform;
    [SerializeField] private RectTransform buttonTransform;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private RectTransform clickTarget;
    [SerializeField] private Animator animator;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    // [System.NonSerialized] private bool _open = false;

    
    public void OnClick_Open()
    {
        if (!GameManager.PLAYING)
        {
            ApplicationManager.Extra = "Playing";
            return;
        }
        ApplicationManager.Extra = "Confirmed";

        background.enabled = false;

        
        if (ONBOARDING.LEARNED_ALL_TABS.IsNotComplete())
        {
            Onboarding.HideFinger();
        }
        
        MenuNavigator.THIS.Open();

        if (ONBOARDING.LEARNED_ALL_TABS.IsNotComplete())
        {
            button.targetGraphic.raycastTarget = false;
            buttonTransform.DOKill();
            buttonTransform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack).SetUpdate(true);
        }
    }

    public bool VisibleImmediate
    {
        set
        {
            buttonTransform.gameObject.SetActive(value);
            if (!value)
            {
                return;
            }
            buttonTransform.localScale = Vector3.one;
            animator.enabled = true;
            button.targetGraphic.raycastTarget = true;
        }
    }
    
    public void AnimatedShow()
    {
        // this._open = true;

        trailRenderer.emitting = false;
        button.targetGraphic.raycastTarget = false;
        this.WaitForNull(Show);
        
        void Show()
        {
            background.enabled = false;
            if (ONBOARDING.LEARNED_ALL_TABS.IsNotComplete())
            {
                background.enabled = true;
                UIManager.Pause(true);
            }
            
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

                VisibleImmediate = true;
                
                buttonTransform.DOKill();
                buttonTransform.DOPunchScale(Vector3.one * 0.2f, 0.25f, 1).SetUpdate(true);
                
                if (ONBOARDING.LEARNED_ALL_TABS.IsNotComplete())
                {
                    Onboarding.ClickOn(clickTarget.position, Finger.Cam.Game, () =>
                    {
                        pivot.DOKill();
                        pivot.localScale = Vector3.one;
                        pivot.DOPunchScale(Vector3.one * 0.2f, 0.3f, 1).SetUpdate(true);
                    }, 0.6f);
                    trailRenderer.Clear();
                }
            };
        }
    }
}
