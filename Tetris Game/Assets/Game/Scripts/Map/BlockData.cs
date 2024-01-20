using UnityEngine;

[CreateAssetMenu(fileName = "Block Data", menuName = "Block/Data", order = 0)]
public class BlockData : ScriptableObject
{
    public bool less = false;
    public Color Color;
    public int FitHeight;
    public int NormalHeight;
    public int NormalWidth;
    public int[] checkAngles;
    public Vector3 spawnerOffset;

}
