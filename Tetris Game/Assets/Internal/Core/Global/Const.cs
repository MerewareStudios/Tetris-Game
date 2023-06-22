using System;
using DG.Tweening;
using Game;
using Game.UI;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Game Const", menuName = "Game/Const", order = 0)]
public class Const : SSingleton<Const>
{
    [Header("Look Up")]
    public LevelSo[] Levels;
    public GunSo[] Guns;
    public Pawn.Usage[] PowerUps;
    
    
    [Header("Defaults")]
    public Player.Data DefaultPlayerData;
    public User.Data DefaultUserData;
    public Gun.UpgradeData[] GunUpgradeData;
    
    [Header("Colors")] 
    public Color defaultColor;
    public Color steadyColor;
    public Color mergerColor;
    public Color[] placeColors;
    public Color mergerPlaceColor;
    public Color[] enemyColors; // enemy colors
    public Color singleColor;
    public Color comboColor;
    public Gradient frontLineGradient;
    
    [Header("Values")] 
    public float rotationDuration = 0.15f;
    public float jumpDuration = 0.15f;
    public Vector3 jumpPower;
    public Ease rotationEase;
}

public static class ConstExtensions
{
    public static Color Health2Color(this int health)
    {
        return Const.THIS.enemyColors[health - 1];
    } 
}