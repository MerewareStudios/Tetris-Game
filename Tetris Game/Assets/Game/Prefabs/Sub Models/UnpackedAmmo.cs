using System.Drawing;
using DG.Tweening;
using Color = UnityEngine.Color;

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
