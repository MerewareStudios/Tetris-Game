using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;

public class FakeAdBanner : Singleton<FakeAdBanner>
{
    [SerializeField] private Canvas canvas;

    public static void Show()
    {
        FakeAdBanner.THIS.canvas.enabled = true;
    }
    
    public static void Hide()
    {
        FakeAdBanner.THIS.canvas.enabled = false;
    }
}
