using UnityEngine;
using UnityEngine.Serialization;

namespace AvatarScripts
{
    public class AvatarScalerTester : MonoBehaviour
    {
        [FormerlySerializedAs("avatarScaler"),SerializeField] SimpleAvatarScaler simpleAvatarScaler;

        [SerializeField, Range(float.Epsilon, 10f)]
        float scale = 1f;

        
        void OnValidate() => simpleAvatarScaler.ChangeSize(scale);
    }
}