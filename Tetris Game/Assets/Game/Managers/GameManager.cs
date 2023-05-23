using Internal.Core;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [SerializeField] public Constants Constants;
    [SerializeField] public static MaterialPropertyBlock MPB_PLACE;

    void Awake()
    {
        MPB_PLACE = new();
    }
}
