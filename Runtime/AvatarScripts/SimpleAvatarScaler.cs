using UnityEngine;
using UnityEngine.Events;

namespace AvatarScripts
{
    public class SimpleAvatarScaler : MonoBehaviour
    {
        [SerializeField, Range(0.1f, 0.5f),] float minSize = 0.2f;
        [SerializeField, Range(2f, 10f),] float maxSize = 4f;
        [SerializeField] CharacterCapsule characterCapsule;

        public UnityEvent<float> sizeChange;
        public float CurrentSize { get; private set; } = 1f;

        Vector3 currentScale = Vector3.one;
        public void ChangeSize(float newSize)
        {
            CurrentSize = Mathf.Clamp(newSize, minSize, maxSize);
            currentScale.Set(CurrentSize,CurrentSize,CurrentSize);
            transform.localScale = currentScale;
            characterCapsule.SetHeight(CurrentSize);
            sizeChange?.Invoke(CurrentSize);
        }
    }
}