using System;
using Unity.Plastic.Newtonsoft.Json.Serialization;
using UnityEngine;

namespace CollsionBasedMovement
{
    public abstract class MoveCharacter : MonoBehaviour
    {
        public enum MoveModes
        {
            Walking,
            Falling,
            Hovering,
            Swimming,
        }

        [Header("Sprint Variables"), SerializeField, Range(1.15f, 2f),]
        float sprintMultiplier = 1.15f;

        [SerializeField, Range(1.5f, 3f),] protected float sprintAcceleration = 2f;
        protected bool sprinting;
        [SerializeField] MoveModes currentMode;

        [field: SerializeField] public Rigidbody Rigid { get; private set; }
        [field: SerializeField] public MoveStatsManager Stats { get; private set; }
        protected float Speed => Stats.MoveSpeed.Value;
        public float MaxSwimSpeed => Speed;
        public event Action<MoveModes> ChangedMode; 
        public MoveModes CurrentMode
        {
            get => currentMode;
            protected set
            {
                currentMode = value;
                ChangedMode?.Invoke(value);
            }
        }

        protected float MaxSpeed => sprinting ? Speed * sprintMultiplier : Speed;
        public bool Swimming => CurrentMode is MoveModes.Swimming;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Rigid == null)
            {
                if (TryGetComponent(out Rigidbody rb))
                    Rigid = rb;
                else
                    throw new MissingComponentException("Missing rigid body");
            }
        }
#endif

        public abstract bool IsCrouching();
        public abstract bool IsGrounded();
        public bool IsSprinting() => sprinting;
        public abstract bool WasGrounded();
        public Vector3 GetVelocity() => Rigid.velocity;
        public float GetCurrentSpeed() => GetVelocity().magnitude;
        public float GetMaxSpeed() => MaxSpeed;
        public abstract Vector3 GetLocalMoveDirection();
        public abstract Vector3 GetUpVector();
    }
}