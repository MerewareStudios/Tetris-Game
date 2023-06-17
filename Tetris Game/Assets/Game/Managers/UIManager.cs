using System;
using Game.UI;
using Internal.Core;
using Internal.Visuals;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UIManager : Singleton<UIManager>
{
   [Header("Menus")]
   [SerializeField] public BlockMenu blockMenu;
   [Header("Counters")]
   [SerializeField] public Counter healthCounter;
   [Header("Flying Text")]
   [SerializeField] public FlyingText ft_TF2;
   [SerializeField] public FlyingText ft_Combo;

   void Awake()
   {
      ft_TF2.OnGetInstance = () => { return Pool.Flying_Text___TF2.Spawn<TextMeshProUGUI>(); };
      ft_TF2.ReturnInstance = (mono) => { mono.Despawn(); };
      
      ft_Combo.OnGetInstance = () => { return Pool.Flying_Text___Combo.Spawn<TextMeshProUGUI>(); };
      ft_Combo.ReturnInstance = (mono) => { mono.Despawn(); };
   }

   public void OpenShop()
   {
      blockMenu.Open();
   }
}
