using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Game.UI;
using Internal.Core;
using UnityEngine;
using UnityEngine.UI;

public class RewardScreen : Singleton<RewardScreen>
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private RectTransform rewardDisplayParent;
    [SerializeField] private ParticleSystem piggyBlowPS;
    [SerializeField] private Button claimButton;
    [System.NonSerialized] private List<RewardDisplay> _rewardDisplays = new ();
    [System.NonSerialized] private bool _canClaim = false;
    
    public void ShowFakeCards()
    {
        List<PiggyMenu.PiggyReward> rewardDatas = new List<PiggyMenu.PiggyReward>();
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.PiggyCoin, 5));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Coin, 10));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        
        Show(rewardDatas);
    }

    public void Show(List<PiggyMenu.PiggyReward> rewardDatas)
    {
        _canvas.enabled = true;
        
        piggyBlowPS.Play();
        _rewardDisplays.Clear();

        float angleAddition = -25.0f / rewardDatas.Count;
        
        for (int i = 0; i < rewardDatas.Count; i++)
        {
            RewardDisplay rewardDisplay = Pool.Reward_Display.Spawn<RewardDisplay>();
            rewardDisplay.rectTransform.SetParent(rewardDisplayParent);
            rewardDisplay.rectTransform.localPosition = Vector3.zero;
            
            rewardDisplay.rectTransform.DOKill();
            
            rewardDisplay.rectTransform.localScale = Vector3.one * 0.15f;
            rewardDisplay.rectTransform.DOScale(Vector3.one * (1.0f - i * 0.02f), 0.1f * i + 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            
            rewardDisplay.rectTransform.localEulerAngles = Vector3.zero;
            rewardDisplay.rectTransform.DORotate(new Vector3(0.0f, 0.0f, angleAddition * i), 0.1f * i + 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            
            rewardDisplay.Set(rewardDatas[i], i);
            
            _rewardDisplays.Add(rewardDisplay);
        }

        claimButton.transform.DOKill();
        claimButton.transform.localScale = Vector3.zero;
        claimButton.transform.DOScale(Vector3.one, 0.25f).SetDelay(0.3f + 0.1f * rewardDatas.Count).SetUpdate(true);

        _canClaim = true;

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            ShowFakeCards();
        }
    }

    public void Deconstruct()
    {
        foreach (var display in _rewardDisplays)
        {
            display.Despawn();
        }
    }


    public void OnClick_Claim()
    {
        if (!_canClaim)
        {
            return;
        }
        if (_rewardDisplays.Count == 0)
        {
            return;
        }

        _canClaim = false;
        
        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(true);
        
        RewardDisplay rewardDisplay = _rewardDisplays[^1];
        _rewardDisplays.RemoveAt(_rewardDisplays.Count - 1);

        const float cardDragDur = 0.4f;

        Tween upTween = rewardDisplay.rectTransform.DOScale(Vector3.one * 1.1f, 0.45f).SetEase(Ease.OutQuad).SetUpdate(true);
        Tween rotationTween = rewardDisplay.rectTransform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
        Tween punchScaleUp = rewardDisplay.rectTransform.DOScale(Vector3.one * 0.125f, 0.15f).SetRelative(true).SetEase(Ease.OutBack).SetUpdate(true);
        upTween.onComplete =
            () =>
            {
                rewardDisplay.ps.Play();
            };
        Tween dragUpTween = rewardDisplay.rectTransform.DOAnchorPosY(750.0f, cardDragDur).SetDelay(0.25f).SetRelative(true).SetEase(Ease.InOutSine).SetUpdate(true);
        dragUpTween.onComplete =
            () =>
            {
                rewardDisplay.SetSortingBehind();
            };
        Tween dragDownTween = rewardDisplay.rectTransform.DOAnchorPosY(-750.0f, cardDragDur).SetRelative(true).SetDelay(cardDragDur).SetEase(Ease.InOutSine).SetUpdate(true);
        Tween scaleDownTween = rewardDisplay.rectTransform.DOScale(Vector3.one * 0.5f, cardDragDur * 2.0f).SetEase(Ease.InOutSine).SetUpdate(true);


        sequence.Append(upTween).Join(rotationTween).Append(punchScaleUp).Append(dragUpTween).Join(scaleDownTween).Join(dragDownTween);
        sequence.onComplete = () =>
        {
            rewardDisplay.Despawn();
        };

        DOVirtual.DelayedCall(0.9f, () =>
        {
            _canClaim = true;
        });
    }
}
