using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Game Const", menuName = "Game/Const", order = 0)]
public class Const : SSingleton<Const>
{
    public LevelSo[] Levels;
    public Color[] enemyColors; // enemy colors
    [Header("Defaults")]
    [Header("Defaults/Player")] [SerializeField]
    public Player.Data DefaultPlayerData;
    [Header("Defaults/Gun")] [SerializeField]
    public GunSo[] GunSos;
    [Header("-----")]
    public float tickInterval = 0.15f;
    public Pool[] blocks;
    [Header("Pawn")] 
    public Color defaultColor;
    public Color steadyColor;
    public Color mergerColor;
    [Header("Colors")] 
    public Color[] placeColors;
    public Color mergerPlaceColor;
    [Header("Spawner")] 
    public float rotationDuration = 0.15f;
    public float jumpDuration = 0.15f;
    public Vector3 jumpPower;
    public Ease rotationEase;
    [Header("Combo")]
    public Color singleColor;
    public Color comboColor;
}

public static class ConstExtensions
{
    public static Color Health2Color(this int health)
    {
        return Const.THIS.enemyColors[health - 1];
    } 
}