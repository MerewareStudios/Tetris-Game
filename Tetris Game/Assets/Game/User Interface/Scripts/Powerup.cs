using DG.Tweening;
using Game;
using Internal.Core;
using IWI;
using UnityEngine;
using UnityEngine.UI;

public class Powerup : Lazyingleton<Powerup>
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform pivot;
    [SerializeField] private Image icon;
    [SerializeField] public RectTransform currencyTarget;
    [SerializeField] private RectTransform leftDoor;
    [SerializeField] private RectTransform rightDoor;
    [SerializeField] private RectTransform lockIcon;
    [SerializeField] private RectTransform currencyDisplay;
    [SerializeField] private RectTransform useDisplay;
    [SerializeField] public RectTransform fingerTarget;
    [SerializeField] public ParticleSystem ps;
    [System.NonSerialized] private bool _canUse = false;
    [System.NonSerialized] private Data _data;
    public bool Available => this._data.available;

    public bool Enabled
    {
        set
        {
            
#if CREATIVE
            if (!Const.THIS.creativeSettings.powerUpEnabled)
            {
                canvas.enabled = false;
                return;
            }
#endif
            
            canvas.enabled = value;
            
#if CREATIVE
    currencyDisplay.gameObject.SetActive(false);
#endif
            
        }
        get => canvas.enabled;
    }

    public Data _Data
    {
        set
        {
            this._data = value;
            OpenImmediate(_data.available);
            SetPowerup(_data.currentUsage, byPassUse:true);

            Enabled = ONBOARDING.USE_POWERUP.IsComplete() || ONBOARDING.WEAPON_TAB.IsComplete();
        }
        get => this._data;
    }
    public void OpenImmediate(bool open)
    {
        _canUse = open;
        
        lockIcon.DOKill();
        lockIcon.localScale = open ? Vector3.zero : Vector3.one;
        leftDoor.anchoredPosition = Vector2.right * (open ?  -75.0f : 0.0f);
        rightDoor.anchoredPosition = Vector2.right * (open ? 75.0f : 0.0f);
        currencyDisplay.anchoredPosition = Vector2.up * (open ? 130.0f : 0.0f);
        useDisplay.anchoredPosition = Vector2.up * (open ? 0.0f : 130.0f);
    }

    public void OpenAnimated(bool open)
    {
        _canUse = false;
        
        lockIcon.DOKill();
        lockIcon.DOScale(open ? Vector3.zero : Vector3.one, 0.25f).SetDelay(open ? 0.0f : 0.1f).SetEase(open ? Ease.InBack : Ease.OutBack).SetUpdate(true);
        
        leftDoor.DOKill();
        leftDoor.DOAnchorPosX( open ? -75.0f : 0.0f,  open ? 0.125f : 0.3f).SetDelay(open ? 0.1f : 0.0f).SetEase(open ? Ease.InCubic : Ease.OutBounce).SetUpdate(true);
        
        rightDoor.DOKill();
        rightDoor.DOAnchorPosX(open ? 75.0f : 0.0f, open ? 0.125f : 0.3f).SetDelay(open ? 0.1f : 0.0f).SetEase(open ? Ease.InCubic : Ease.OutBounce).SetUpdate(true);

        currencyDisplay.DOKill();
        currencyDisplay.DOAnchorPosY(open ? 130.0f : 0.0f, 0.25f).SetDelay(open ? 0.0f : 0.2f)
            .SetEase(open ? Ease.InBack : Ease.OutBack, 2.0f).SetUpdate(true);

        useDisplay.DOKill();
        useDisplay.DOAnchorPosY(open ? 0.0f : 130.0f, 0.25f).SetDelay(open ? 0.2f : 0.0f).SetUpdate(true)
            .SetEase(open ? Ease.OutBack : Ease.InBack).onComplete = () =>
        {
            _canUse = open;
        };

        PunchUse();
    }

    public void SetPowerup(Pawn.Usage usage, bool punch = false, bool byPassUse = false)
    {
        this._Data.currentUsage = usage;
        icon.enabled = true;
        icon.sprite = usage.PowerUpIcon();
        if (punch)
        {
            PunchUse();
        }

        if (byPassUse)
        {
            return;
        }

        if (_Data.use)
        {
            _canUse = true;
            OnClick_Use();
        }
    }

    public void PunchUse()
    {
        icon.rectTransform.DOKill();
        icon.rectTransform.localScale = Vector3.one;
        icon.rectTransform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 1).SetUpdate(true);

        PunchFrame();
    }

    public void PunchFrame(float amount = 0.4f)
    {
        pivot.DOKill();
        pivot.localScale = Vector3.one;
        pivot.DOPunchScale(Vector3.one * amount, 0.3f, 1).SetUpdate(true);

        if (Enabled)
        {
            ps.Emit(1);
        }
    }
    private void PunchCost(float amount = 35.0f)
    {
        currencyDisplay.DOKill();
        currencyDisplay.localPosition = Vector3.zero;
        currencyDisplay.DOPunchAnchorPos(Vector3.up * amount, 0.25f, 1).SetUpdate(true);
    }
    public void OnClick_Use()
    {
        if (!GameManager.PLAYING)
        {
            return;
        }
        
        HapticManager.OnClickVibrate();


        if (Available)
        {
            if (!_canUse)
            {
                return;   
            }

            if (_data.currentUsage.Equals(Pawn.Usage.Empty))
            {
                PowerSelectionScreen.THIS.Toggle();
                return;
            }
            
            Audio.Powerup_Open.Play();
            
            _data.available = false;
            Spawner.THIS.InterchangeBlock(Pool.Single_Block, this._Data.currentUsage);
            
            if (ONBOARDING.USE_POWERUP.IsNotComplete())
            {
                ONBOARDING.USE_POWERUP.SetComplete();
                Onboarding.Deconstruct();
            }

            PunchFrame(0.2f);
            icon.enabled = false;
            OpenAnimated(false);
        }
        else
        {
            PunchFrame(0.1f);

            if (Try2Use())
            {
                return;
            }
            
            PunchCost();
            AdManager.ShowTicketAd(AdBreakScreen.AdReason.POWERUP,() =>
            {
                Wallet.Transaction(Const.Currency.OneAd);
                Try2Use();
            });
        }
    }

    private bool Try2Use()
    {
        if (!Wallet.Consume(Const.Currency.OneAd))
        {
            return false;
        }
        
        _data.available = true;
        SetPowerup(Pawn.Usage.Empty, byPassUse:true);
        PunchCost(50.0f);
        PowerSelectionScreen.THIS.Open();

        OpenAnimated(true);

        
        _Data.purchaseCount++;
        AnalyticsManager.PurchasedPower(_Data.purchaseCount);
        
        return true;
    }

    [System.Serializable]
    public class Data : System.ICloneable
    {
        [SerializeField] public bool available = false;
        [SerializeField] public Pawn.Usage currentUsage;
        [SerializeField] public int purchaseCount;
        [SerializeField] public bool use;
        public Data()
        {
            
        }
        public Data(Data data)
        {
            this.available = data.available;
            this.currentUsage = data.currentUsage;
            this.purchaseCount = data.purchaseCount;
            this.use = data.use;
        }
        public object Clone()
        {
            return new Data(this);
        }
    } 
}
