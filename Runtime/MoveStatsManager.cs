using System;
using MovementScripts;
using UnityEngine;

public class MoveStatsManager : MoveStats
{
    [ SerializeField]  FloatStat moveSpeed  = new(10);
    [ SerializeField]  FloatStat swimmingSpeed  = new(8);
    [ SerializeField]  FloatStat sprintMultiplier  = new(8);
    [SerializeField] FloatStat jumpStrength = new FloatStat(2f);
    [SerializeField, Range(1, 4)] int jumpCount = 2;
    public override int MaxJumpCount => jumpCount;
    public override float JumpStrength => jumpStrength.Value;
    public override float SprintMultiplier => sprintMultiplier.Value;
    public override float SwimSpeed => swimmingSpeed.Value;
    public override float WalkSpeed => moveSpeed.Value;

    public override void AddMod(MoveCharacter.MoveModes mode, FloatMod speedMod)
    {
        switch (mode)
        {
            case MoveCharacter.MoveModes.Walking:
                moveSpeed.AddMod(speedMod);
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
                moveSpeed.RemoveMod(speedMod);
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