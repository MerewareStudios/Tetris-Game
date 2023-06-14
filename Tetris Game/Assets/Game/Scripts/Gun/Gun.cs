using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] public Transform muzzle; 
    [System.NonSerialized] private Data _data;

    
    public Data _Data
    {
        set
        {
            _data = value;
            
            transform.Set(_data.type.GetTransformData());
        }
        get => _data;
    }
    
    [System.Serializable]
    public class Data : ICloneable
    {
        [SerializeField] public Type type;
        [SerializeField] public float shootInterval = 0.5f;
        [SerializeField] public float prevShoot = 0.0f;
        [SerializeField] public int maxShootPerTrigger = 1;

            
        public Data()
        {
            this.shootInterval = 0.5f;
            this.maxShootPerTrigger = 1;
            this.prevShoot = 0.0f;
        }
        public Data(Data data)
        {
            this.type = data.type;
            this.shootInterval = data.shootInterval;
            this.prevShoot = data.prevShoot;
            this.maxShootPerTrigger = data.maxShootPerTrigger;
        }

        public object Clone()
        {
            return new Data(this);
        }
    }

    [Serializable]
    public enum Type
    {
        GUN1,
        GUN2,
        GUN3,
        GUN4,
        GUN5,
    }
}
