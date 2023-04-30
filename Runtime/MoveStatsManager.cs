using CollsionBasedMovement;
using UnityEngine;

public class MoveStatsManager : MonoBehaviour,IMoveStats
{
    [field: SerializeField] public FloatStat MoveSpeed { get; private set; } = new(10);
    [field: SerializeField] public FloatStat SwimmingSpeed { get; private set; } = new(8);
    public int MaxJumpCount => 2;
    public float JumpStrength => 2f;
    public float SprintMultiplier => 1.5f;
    public float SwimSpeed => SwimmingSpeed.Value;
    public float WalkSpeed => MoveSpeed.Value;
}