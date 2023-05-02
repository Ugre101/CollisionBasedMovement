using UnityEngine;

namespace AvatarScripts
{
    public class AvatarScalerTester : MonoBehaviour
    {
        [SerializeField] AvatarScaler avatarScaler;

        [SerializeField, Range(float.Epsilon, 10f)]
        float scale = 1f;

        void OnValidate() => avatarScaler.ChangeSize(scale);
    }
}