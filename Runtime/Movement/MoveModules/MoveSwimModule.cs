using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CollsionBasedMovement.MoveModules
{
    [Serializable]
    public class MoveSwimModule : MoveBaseModule
    {
        [Header("Swimming Variables"), SerializeField, Range(0.1f, 2f),]
        
        float swimmingBuoyancy = 1f;

        [SerializeField, Range(3f, 10f),] float swimmingMaxBuoyancy = 5f;
        [SerializeField, Range(0f, 0.5f),] float swimmingSweetZone = 0.5f;
        [SerializeField, Range(0f, 0.5f),] float swimmingSweetZoneYDownBraking = 0.1f;
        [SerializeField, Range(0.25f, 0.75f),] float swimmingSweetZoneYUpBraking = 0.1f;
        [SerializeField, Range(0f, 1f),] float percentSubmerged = 0.7f;

        [Header("Dive Variables"), SerializeField, Range(2f, 10f),]
        
        float diveForce = 1f;

        [SerializeField, Range(5f, 20f),] float maxDiveSpeed = 15f;
        [SerializeField, Range(0.05f, 0.25f),] float diveUpForceBreak = 0.25f;

        [SerializeField, Range(0f, 0.1f),] float brakeForce = 0.075f;
        bool diving;
        float waterLine;

        public override void OnEnter(Collider collider)
        {
            waterLine = collider.bounds.max.y;
        }

        protected override void OnGravity()
        {
            if (IsDiving())
                return;
            if (IsUnderWaterNotDiving())
                return;
            InSwimmingSweetZone();
        }

        void InSwimmingSweetZone()
        {
            var vel = rigid.velocity;
            switch (vel.y)
            {
                case < -0.1f:
                    vel.y *= 1f - swimmingSweetZoneYDownBraking;
                    break;
                case > 0.1f:
                    vel.y *= 1f - swimmingSweetZoneYUpBraking;
                    break;
            }

            rigid.velocity = vel;
        }

        bool IsDiving()
        {
            if (!diving)
                return false;

            rigid.AddForce(Vector3.down * diveForce, ForceMode.Force);
            var limited = rigid.velocity;
            if (rigid.velocity.y > 1f)
                limited.y *= 1f - diveUpForceBreak;
            else if (rigid.velocity.y <= -maxDiveSpeed)
                limited.y = -maxDiveSpeed;

            rigid.velocity = limited;

            return true;
        }

        bool IsUnderWaterNotDiving()
        {
            var yDiff = waterLine - (capsule.YMin + capsule.Height * percentSubmerged);
            if (Mathf.Abs(yDiff) < swimmingSweetZone)
                return false;
            var buoyancy = Mathf.Clamp(yDiff * swimmingBuoyancy, -swimmingMaxBuoyancy, swimmingMaxBuoyancy);
            rigid.AddForce(Vector3.up * buoyancy, ForceMode.Force);
            return true;
        }

        public override void OnMove(Vector3 force, bool sprinting, float sprintAcc)
        {
            if (sprinting)
                force *= sprintAcc;
            rigid.AddForce(force, ForceMode.Force);
        }

        public override void OnJump(InputAction.CallbackContext ctx)
        {
        }

        public override void OnCrunch(InputAction.CallbackContext ctx)
        {
            if (ctx.started || ctx.performed)
                diving = true;
            else if (ctx.canceled)
                diving = false;
        }

        public override void ApplyBraking()
        {
            var brakedForce = rigid.velocity;
            var breakValue = 1f - brakeForce;
            if (rigid.velocity.x != 0)
                brakedForce.x *= breakValue;
            if (rigid.velocity.z != 0)
                brakedForce.z *= breakValue;
            if (rigid.velocity.y != 0)
                brakedForce.y *= breakValue;
            rigid.velocity = brakedForce;
        }
    }
}