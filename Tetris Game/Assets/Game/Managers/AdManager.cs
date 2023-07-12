using System;
using System.Collections;
using System.Collections.Generic;
using Internal.Core;
using UnityEngine;


namespace  IWI
{
    public class AdManager : Singleton<AdManager>
    {
        [System.NonSerialized] private Data _data;

        public Data _Data
        {
            set
            {
                _data = value;
            }
            get => _data;
        }
        
        [System.Serializable]
        public class Data : ICloneable
        {
            [SerializeField] private long loanBarStamp;
            
            public Data()
            {
                    
            }
            public Data(Data data)
            {
                this.loanBarStamp = data.loanBarStamp;
            }

            public int LoanBarSecondsLeft => (int)TimeSpan.FromTicks((loanBarStamp - DateTime.Now.Ticks)).TotalSeconds; 
            public bool CanUseLoanBar => LoanBarSecondsLeft <= 0; 
            public void StampLoanBar()
            {
                loanBarStamp = DateTime.Now.Ticks + TimeSpan.FromSeconds(Const.THIS.loanBarInterval).Ticks;
            }

            public object Clone()
            {
                return new Data(this);
            }
        } 
        
    }
}
