using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace AvatarScripts
{
    public class AvatarScaler : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 0.5f),] float minSize = 0.2f;
        [SerializeField, Range(2f, 10f),] float maxSize = 4f;
        [SerializeField] CharacterCapsule characterCapsule;

        public UnityEvent<float> sizeChange;
        public float CurrentSize { get; private set; } = 1f;

        public void ChangeSize(float newSize)
        {
            CurrentSize = Mathf.Clamp(newSize, minSize, maxSize);
            transform.localScale.Set(CurrentSize, CurrentSize, CurrentSize);
            characterCapsule.SetHeight(CurrentSize);
            sizeChange?.Invoke(CurrentSize);
        }
    }
}