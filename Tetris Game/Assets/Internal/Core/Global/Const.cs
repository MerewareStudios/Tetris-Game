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
    public Color piggyExplodeColor;
    
    [Header("Visuals")] 
    public DirectionRadiusPair[] rewardDirectionRadiusPair;
    
    [Header("Values")] 
    public float rotationDuration = 0.15f;
    public float jumpDuration = 0.15f;
    public Vector3 jumpPower;
    public Ease rotationEase;
    public Ease piggyExplodeEase;
    
    [Serializable]
    public struct DirectionRadiusPair
    {
        [SerializeField] public Vector3 dir;
        [SerializeField] public float radius;
    }
}

public static class ConstExtensions
{
    public static Color Health2Color(this int health)
    {
        return Const.THIS.enemyColors[health - 1];
    } 
    public static Vector3 Direction(this int rewardCount)
    {
        return Const.THIS.rewardDirectionRadiusPair[rewardCount - 1].dir;
    } 
    public static float Radius(this int rewardCount)
    {
        return Const.THIS.rewardDirectionRadiusPair[rewardCount - 1].radius;
    }
}