using Game;
using Internal.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    public void SwipeLeft()
    {
        if (Map.THIS.currentBlock != null)
        {
            //Map.THIS.currentBlock.Move(Block.Direction.LEFT);
            Map.THIS.currentBlock.QueueDirection(Block.Direction.LEFT);
        }
    }
    public void SwipeRight()
    {
        if (Map.THIS.currentBlock != null)
        {
            //Map.THIS.currentBlock.Move(Block.Direction.RIGHT);
            Map.THIS.currentBlock.QueueDirection(Block.Direction.RIGHT);
        }
    }
    public void SwipeUp()
    {
        if (Map.THIS.currentBlock != null)
        {
            //Map.THIS.currentBlock.Move(Block.Direction.RIGHT);
            Map.THIS.currentBlock.QueueDirection(Block.Direction.FORWARD);
        }
    }
    public void SwipeDown()
    {

    }
}
