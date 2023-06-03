using System;
using Internal.Core;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
   [SerializeField] public FlyingText ft_TF2;
   [SerializeField] public FlyingText ft_Combo;

   void Awake()
   {
      ft_TF2.OnGetInstance = () => { return Pool.Flying_Text___TF2.Spawn<TextMeshProUGUI>(); };
      ft_TF2.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Combo.OnGetInstance = () => { return Pool.Flying_Text___Combo.Spawn<TextMeshProUGUI>(); };
      ft_Combo.ReturnInstance = (mono) => { mono.Despawn(); };
   }
}
