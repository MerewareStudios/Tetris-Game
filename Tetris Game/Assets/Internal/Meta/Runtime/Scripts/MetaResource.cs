using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaResource : MonoBehaviour
{
    public enum Type
    {
        TYPE1,
        TYPE2,
        TYPE3,
        TYPE4,
        TYPE5,
    }

    [System.Serializable]
    public struct Requirements : Meta.IMeta
    {
        public Required[] requiredList;

        public List<(int, int)> GetData()
        {
            List<(int, int)> values = new();
            foreach (Required required in requiredList)
            {
                values.Add((((int)required.type), required.amount));
            }

            return values;
        }
    }
    [System.Serializable]
    public struct Required
    {
        public Type type;
        public int amount;
    }
}
