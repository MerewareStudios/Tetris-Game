using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    public void OnClick_Use()
    {
        Spawner.THIS.InterchangeBlock(Pool.Single_Block, Pawn.Usage.MagnetLR);

    }
}
