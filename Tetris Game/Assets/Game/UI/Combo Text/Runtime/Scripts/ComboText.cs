using DG.Tweening;
using Internal.Core;
using TMPro;
using UnityEngine;

namespace  Game.UI
{
    public class ComboText : MonoBehaviour
    {
        [SerializeField] private RectTransform multTransform;
        [SerializeField] private RectTransform countTransform;
        [SerializeField] private TextMeshProUGUI multText;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private ParticleSystem ps1;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color fadeColor;
        [System.NonSerialized] private Sequence _comboSequence = null;
        [SerializeField] private float animDuration;
        [SerializeField] private float firstPrepend;
        [SerializeField] private float secondPrepend;
        
        public float Show(int combo, float extraWait = 1.0f)
        {
            _comboSequence?.Kill();
            multTransform.DOKill();
            countTransform.DOKill();
            multText.DOKill();
            countText.DOKill();
            
            countText.text = combo.ToString();

            multText.color = fadeColor;
            countText.color = fadeColor;

            multTransform.localScale = Vector3.one * 3.0f;
            countTransform.localScale = Vector3.one * 3.0f;
            
            ps1.Play();

            
            Tween multAlphaTween = multText.DOColor(normalColor, animDuration).SetEase(Ease.InSine);
            Tween multScaleDownTween = multTransform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutQuad);

            Tween countAlphaTween = countText.DOColor(normalColor, animDuration).SetEase(Ease.InSine).SetDelay(firstPrepend);
            Tween countScaleDownTween = countTransform.DOScale(Vector3.one, animDuration).SetEase(Ease.OutQuad);

            Tween multFadeOut = multText.DOColor(fadeColor, animDuration).SetEase(Ease.InSine);
            Tween countFadeOut = countText.DOColor(fadeColor, animDuration).SetEase(Ease.InSine);
            
            Tween multScaleUpTween = multTransform.DOScale(Vector3.one * 1.25f, animDuration).SetEase(Ease.OutQuad);
            Tween countScaleUpTween = countTransform.DOScale(Vector3.one * 1.25f, animDuration).SetEase(Ease.OutQuad);

            _comboSequence = DOTween.Sequence();
            _comboSequence.Join(multAlphaTween);
            _comboSequence.Join(multScaleDownTween);
            _comboSequence.Join(countAlphaTween);
            _comboSequence.Join(countScaleDownTween);
            _comboSequence.AppendInterval(secondPrepend * extraWait);
            _comboSequence.Append(multFadeOut);
            _comboSequence.Join(countFadeOut);
            _comboSequence.Join(multScaleUpTween);
            _comboSequence.Join(countScaleUpTween);

            _comboSequence.onComplete = ps1.Stop;

            return _comboSequence.Duration();
        }
    }
}
