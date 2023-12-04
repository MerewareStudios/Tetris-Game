using DG.Tweening;
using UnityEngine;

public class UnpackedAmmo : SubModel
{
    public override int GetExtra()
    {
        return 1;
    }

    public override void OnMerge()
    {
        base.OnMerge();
        
        meshRenderer.material.DOGradientColor(Const.THIS.mergeGradient, 0.25f).SetEase(Ease.Linear);
    }
}
