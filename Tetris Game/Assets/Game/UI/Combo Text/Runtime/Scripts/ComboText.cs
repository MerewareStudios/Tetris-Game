using DG.Tweening;
using TMPro;
using UnityEngine;

namespace  Game.UI
{
    public class ComboText : MonoBehaviour
    {
        [SerializeField] private RectTransform multTransform;
        [SerializeField] private RectTransform countTransform;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private ParticleSystem ps;
        [System.NonSerialized] private Sequence _comboSequence = null;
        
        public float Show(int combo)
        {
            _comboSequence?.Kill();
            
            // comboText.text = "x" + value;
            //
            // RectTransform comboTextRect = comboText.rectTransform;
            //
            // comboText.color = Color.white;
            // comboTextRect.transform.DOKill();
            // comboTextRect.localScale = Vector3.zero;
            //
            // Tween scaleUp = comboTextRect.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack).SetDelay(0.05f);
            // Tween scaleDown = comboTextRect.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InCirc).SetDelay(0.25f);
            // Tween colorTween = comboText.DOColor(new Color(1.0f, 1.0f, 1.0f, 0.0f), 0.2f).SetEase(Ease.InCirc);

            // _comboSequence = DOTween.Sequence();
            // _comboSequence.Append(scaleUp);
            // _comboSequence.Append(scaleDown);
            // _comboSequence.Join(colorTween);

            return _comboSequence.Duration();

            return 1.0f;
        }
    }
}
