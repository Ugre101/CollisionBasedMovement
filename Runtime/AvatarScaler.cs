using UnityEngine;
using UnityEngine.Events;

public class AvatarScaler : MonoBehaviour
{
    [SerializeField, Range(0.1f, 0.5f),] float minSize = 0.2f;
    [SerializeField, Range(2f, 10f),] float maxSize = 4f;
    [SerializeField] CharacterCapsule characterCapsule;

    public UnityEvent<float> SizeChange;
    public float CurrentSize { get; } = 1f;

    public void ChangeSize(float newSize)
    {
        var size = Mathf.Clamp(newSize, minSize, maxSize);
        transform.localScale.Set(CurrentSize, CurrentSize, CurrentSize);
        SizeChange?.Invoke(CurrentSize);
    }
}