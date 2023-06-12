using DG.Tweening;
using Game;
using UnityEngine;
using UnityEngine.Serialization;


[CreateAssetMenu(fileName = "Game Const", menuName = "Game/Const", order = 0)]
public class Const : SSingleton<Const>
{
    [Header("Defaults")]
    [Header("Defaults/Player")] [SerializeField]
    public Player.Data DefaultPlayerData;
    [Header("-----")]
    public float tickInterval = 0.15f;
    public Pool[] blocks;
    [Header("Pawn")] public Color spawnColor;
    public Color moverColor;
    public Color steadyColor;
    public Color enemyColor;
    public Color bigColor;
    [Header("Colors")] 
    public Color[] placeColors;
    public Color mergerColor;
    [Header("Spawner")] 
    public float rotationDuration = 0.15f;
    public float jumpDuration = 0.15f;
    public Vector3 jumpPower;
    public Ease rotationEase;
    [Header("Combo")]
    public Color singleColor;
    public Color comboColor;
}
