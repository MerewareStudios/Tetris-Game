using Internal.Core;
using System.Collections.Generic;
using UnityEngine;

public class Meta : Singleton<Meta>
{
    [SerializeField] private MetaVisual metaVisualPrefab;
    [SerializeField] public bool godMode;
    [System.NonSerialized] private List<MetaVisual> metaVisuals = new();
    [System.NonSerialized] public System.Action OnTransaction;

    public delegate int Load(int key);
    public static Load OnLoad;
    public delegate void Save(int key, int amount);
    public static Save OnSave;
    public delegate int Convert(System.Enum @enum);
    public static Convert OnConvert;
    public delegate string[] GetEnums();

    public void SetUp(GetEnums OnGetEnums)
    {
        string[] list = OnGetEnums.Invoke();
        for (int i = 0; i < list.Length; i++)
        {
            MetaVisual metaVisual = MonoBehaviour.Instantiate(metaVisualPrefab, this.transform);
            metaVisuals.Add(metaVisual);
            metaVisual.Key = list[i].ToTMProKey();
            metaVisual.Amount = OnLoad.Invoke(i);
        }
    }

    public bool Transaction(System.Enum @meta, int amount)
    {
        int index = OnConvert.Invoke(meta);
        return Transaction(index, amount);
    }
    public bool Transaction(int index, int amount)
    {
        int current = OnLoad.Invoke(index);
        current += amount;
        OnSave.Invoke(index, current);
        OnTransaction?.Invoke();
        Meta.THIS.metaVisuals[index].Amount = current;
        return true;
    }
    public void Spend(IMeta imeta)
    {
        List<(int, int)> values = imeta.GetData();

        foreach (var item in values)
        {
            Transaction(item.Item1, -item.Item2);
        }
    }
    public int Current(System.Enum @meta)
    {
        int index = OnConvert.Invoke(meta);
        int current = OnLoad.Invoke(index);
        return current;
    }
    public bool Has(System.Enum @meta, int amount)
    {
        int index = OnConvert.Invoke(meta);
        int current = OnLoad.Invoke(index);
        return current >= amount;
    }
    public bool Has(int index, int amount)
    {
        int current = OnLoad.Invoke(index);
        return current >= amount;
    }

    public bool HasRequirements(IMeta imeta)
    {
        if (godMode)
        {
            return true;
        }
        List<(int, int)> values = imeta.GetData();

        foreach (var item in values)
        {
            if (!Has(item.Item1, item.Item2))
            {
                return false;
            }
        }
        return true;
    }
    public bool ShowRequirements(IMeta imeta)
    {
        List<(int, int)> values = imeta.GetData();

        HideAll();
        foreach (var value in values)
        {
            metaVisuals[value.Item1].SetAlpha(1.0f);
        }
        return true;
    }
    public void ShowAll()
    {
        foreach (var metaVisual in metaVisuals)
        {
            metaVisual.Hide(false);
        }
    }
    public void HideAll()
    {
        foreach (var metaVisual in metaVisuals)
        {
            metaVisual.Hide(true);
        }
    }

    public interface IMeta
    {
        public List<(int, int)> GetData();
    }
}

public static class MetaExtenstions
{
    public static bool Transaction(this System.Enum @meta, int amount)
    {
        return Meta.THIS.Transaction(@meta, amount);
    }
    public static int Current(this System.Enum @meta)
    {
        return Meta.THIS.Current(@meta);
    }
    public static bool Has(this System.Enum @meta, int amount)
    {
        return Meta.THIS.Has(@meta, amount);
    }
}
