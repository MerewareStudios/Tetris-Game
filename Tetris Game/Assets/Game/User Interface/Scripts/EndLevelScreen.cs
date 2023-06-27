using System.Collections;
using System.Collections.Generic;
using Game.UI;
using UnityEngine;

public class EndLevelScreen : Menu<EndLevelScreen>, IMenu
{
    [SerializeField] private GameObject piggyBankParent;
    [SerializeField] private GameObject optionsParent;
    public new bool Open(float duration = 0.5f)
    {
        if (base.Open(duration))
        {
            return true;
        }
        Show();
        return false;
    }

    private void Show()
    {
        optionsParent.SetActive(true);
    }

    public void OnClick_PiggyBank()
    {
        optionsParent.SetActive(false);
        piggyBankParent.SetActive(true);

    }
    
    public void OnClick_Keep()
    {
        if (base.Close())
        {
            return;
        }
    }
}
