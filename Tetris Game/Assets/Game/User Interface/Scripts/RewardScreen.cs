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
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform rewardDisplayParent;
    [SerializeField] private ParticleSystem piggyBlowPS;
    [SerializeField] private Button claimButton;
    [System.NonSerialized] private List<RewardDisplay> _rewardDisplays = new ();
    [System.NonSerialized] private bool _canClaim = false;
    
    public void ShowFakeCards()
    {
        List<PiggyMenu.PiggyReward> rewardDatas = new List<PiggyMenu.PiggyReward>();
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Coin, 1));
        rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.PiggyCoin, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Ad, 5));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Shield, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Heart, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Medkit, 10));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Protection, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.MaxStack, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.PiggyCapacity, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Damage, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Firerate, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Splitshot, 1));
        // rewardDatas.Add(new PiggyMenu.PiggyReward(PiggyMenu.PiggyReward.Type.Weapon, 1));
        
        Show(rewardDatas);
    }

    public void Show(List<PiggyMenu.PiggyReward> rewardDatas)
    {
        _canvas.enabled = true;
        
        piggyBlowPS.Play();
        _rewardDisplays.Clear();

        float angleAddition = -20.0f / rewardDatas.Count;
        float scaleF = 1.0f - 0.01f * rewardDatas.Count;
        
        for (int i = 0; i < rewardDatas.Count; i++)
        {
            RewardDisplay rewardDisplay = Pool.Reward_Display.Spawn<RewardDisplay>();
            rewardDisplay.rectTransform.SetParent(rewardDisplayParent);
            rewardDisplay.rectTransform.localPosition = Vector3.zero;
            
            rewardDisplay.rectTransform.DOKill();
            
            rewardDisplay.rectTransform.localScale = Vector3.one * 0.15f;
            rewardDisplay.rectTransform.DOScale(Vector3.one * (scaleF + i * 0.01f), 0.1f * i + 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            
            rewardDisplay.rectTransform.localEulerAngles = Vector3.zero;
            rewardDisplay.rectTransform.DORotate(new Vector3(0.0f, 0.0f, 20.0f + angleAddition * i), 0.1f * i + 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
            
            rewardDisplay.Set(rewardDatas[i], i);
            
            _rewardDisplays.Add(rewardDisplay);
        }

        claimButton.transform.DOKill();
        claimButton.transform.localScale = Vector3.zero;
        claimButton.transform.DOScale(Vector3.one, 0.25f).SetDelay(0.3f + 0.1f * rewardDatas.Count).SetEase(Ease.OutBack).SetUpdate(true);

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
        claimButton.transform.DOKill();
        claimButton.transform.localScale = Vector3.one;
        claimButton.transform.DOPunchScale(Vector3.one * -0.2f, 0.35f, 1).SetUpdate(true);
        
        if (!_canClaim)
        {
            return;
        }
        if (_rewardDisplays.Count == 0)
        {
            return;
        }
        
        
        // claimButton.transform.DOKill();
        // claimButton.transform.DOScale(Vector3.one * 0.9f, 0.1f).SetUpdate(true);

        _canClaim = false;
        
        Sequence sequence = DOTween.Sequence();
        sequence.SetUpdate(true);
        
        RewardDisplay rewardDisplay = _rewardDisplays[^1];
        
        bool lastOne = _rewardDisplays.Count == 1;

        if (!lastOne)
        {
            _rewardDisplays.RemoveAt(_rewardDisplays.Count - 1);
        }



        const float cardDragDur = 0.4f;

        Tween upTween = rewardDisplay.rectTransform.DOScale(Vector3.one * 1.1f, 0.45f).SetEase(Ease.OutQuad).SetUpdate(true);
        Tween rotationTween = rewardDisplay.rectTransform.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.OutQuad).SetUpdate(true);
        Tween punchScaleUp = rewardDisplay.rectTransform.DOScale(Vector3.one * 0.3f, 0.15f).SetRelative(true).SetEase(Ease.OutBack).SetUpdate(true);
        upTween.onComplete =
            () =>
            {
                rewardDisplay.ps.Play();
            };
        


        sequence.Append(upTween).Join(rotationTween).Append(punchScaleUp);

        
        DOVirtual.DelayedCall(0.9f, () =>
        {
            if (lastOne)
            {
                _canvasGroup.DOFade(0.0f, 0.25f).SetEase(Ease.InOutSine).SetUpdate(true).onComplete = () =>
                {
                    Deconstruct();
                    this._canvas.enabled = false;
                };
                
            }
            else
            {
                _canClaim = true;
            }
        });
        
        if (lastOne)
        {
            return;
        }
        
        Tween dragUpTween = rewardDisplay.rectTransform.DOAnchorPosY(750.0f, cardDragDur).SetDelay(0.325f).SetRelative(true).SetEase(Ease.InOutSine).SetUpdate(true);
        dragUpTween.onComplete =
            () =>
            {
                rewardDisplay.SetSortingBehind();
            };
        Tween dragDownTween = rewardDisplay.rectTransform.DOAnchorPosY(-750.0f, cardDragDur).SetRelative(true).SetDelay(cardDragDur).SetEase(Ease.InOutSine).SetUpdate(true);
        Tween scaleDownTween = rewardDisplay.rectTransform.DOScale(Vector3.one * 0.5f, cardDragDur * 2.0f).SetEase(Ease.InOutSine).SetUpdate(true);
        
        sequence.Append(dragUpTween).Join(scaleDownTween).Join(dragDownTween);
            
        sequence.onComplete = () =>
        {
            rewardDisplay.Despawn();
        };
            
        
    }
}
