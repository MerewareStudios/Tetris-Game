using UnityEngine;

namespace HomaGames.HomaBelly.DataPrivacy
{
    [AddComponentMenu("")]
    internal class MarkAsDontDestroyOnLoad : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}