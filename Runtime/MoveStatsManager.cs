using System;
using CollsionBasedMovement;
using UnityEngine;

public class MoveStatsManager : MoveStats
{
    [field: SerializeField] public FloatStat MoveSpeed { get; private set; } = new(10);
    [field: SerializeField] public FloatStat SwimmingSpeed { get; private set; } = new(8);
    public override int MaxJumpCount => 2;
    public override float JumpStrength => 2f;
    public override float SprintMultiplier => 1.5f;
    public override float SwimSpeed => SwimmingSpeed.Value;
    public override float WalkSpeed => MoveSpeed.Value;

    public override void AddMod(MoveCharacter.MoveModes mode, FloatMod speedMod)
    {
        switch (mode)
        {
            case MoveCharacter.MoveModes.Walking:
                MoveSpeed.AddMod(speedMod);
                break;
            case MoveCharacter.MoveModes.Falling:
                break;
            case MoveCharacter.MoveModes.Hovering:
                break;
            case MoveCharacter.MoveModes.Swimming:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }

    public override void RemoveMod(MoveCharacter.MoveModes mode, FloatMod speedMod)
    {
        switch (mode)
        {
            case MoveCharacter.MoveModes.Walking:
                MoveSpeed.RemoveMod(speedMod);
                break;
            case MoveCharacter.MoveModes.Falling:
                break;
            case MoveCharacter.MoveModes.Hovering:
                break;
            case MoveCharacter.MoveModes.Swimming:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }
    }
}