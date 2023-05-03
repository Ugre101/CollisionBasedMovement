using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementScripts.MoveModules
{
    [Serializable]
    public class MoveHoverModule : MoveBaseModule
    {
        [SerializeField, Range(0.5f, 2f),] float hoverHeight = 2f;
        [SerializeField, Range(1f, 5f),] float hoverForce = 2f;

        [SerializeField, Range(float.Epsilon, 3f),]
        float fallSpeed = 2f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float sweetZone = 0.5f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float sweetZoneBraking = 0.25f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float jumpForce = 0.5f;

        [SerializeField, Range(0f, 0.15f),] float brakeForce = 0.1f;
        bool crunching;
        bool inSweetZone;
        bool jumping;

        float jumpTimeLeft = 3f;
        float targetHeight;

        public override void OnEnter(Collider collider)
        {
            base.OnEnter(collider);
            targetHeight = capsule.Height * hoverHeight;
        }

        public override void OnCapsuleSizeChange(float newHeight)
        {
            targetHeight = capsule.Height * hoverHeight;
        }

        protected override void OnGravity()
        {
            if (crunching)
            {
                rigid.AddForce(Physics.gravity);
                return;
            }

            var dist = checker.DistanceToGround;
            var terrainHeight = Terrain.activeTerrain.SampleHeight(rigid.position);
            var posY = rigid.position.y - Terrain.activeTerrain.GetPosition().y;
            Debug.Log($"{posY} < {terrainHeight}");
            if (posY < terrainHeight) Debug.Log("In hole?");

            if (posY > terrainHeight + targetHeight * 1.5f) Debug.Log("Hovering on object?");


            var diff = Mathf.Clamp(targetHeight - dist, -10, 10);
            var absDiff = Mathf.Abs(diff);


            AddHoverForce(diff);
            inSweetZone = absDiff < sweetZone;
            if (inSweetZone && !jumping)
                SweetZoneYBraking();
        }

        void AddHoverForce(float diff)
        {
            var addForce = Vector3.up * diff;
            addForce *= diff >= 0 ? hoverForce : fallSpeed;
            rigid.AddForce(addForce, ForceMode.Force);
        }

        void SweetZoneYBraking()
        {
            var limited = rigid.velocity;
            limited.y *= 1f - sweetZoneBraking;
            rigid.velocity = limited;
        }

        public override void OnMove(Vector3 force, bool sprinting, float sprintAcc)
        {
            force *= stats.WalkSpeed;
            if (checker.IsGrounded && sprinting)
                force *= sprintAcc;
            rigid.AddForce(force, ForceMode.Force);
            if (jumping && jumpTimeLeft > 0)
            {
                var distMod = 1 / checker.DistanceToGround;
                distMod = Mathf.Clamp(distMod, float.Epsilon, 2f);
                rigid.AddForce(Vector3.up * (jumpForce * distMod), ForceMode.Impulse);
                jumpTimeLeft -= Time.deltaTime;
            }
            else if (jumpTimeLeft < 3f)
            {
                jumpTimeLeft += Time.deltaTime;
            }
        }

        public override void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started || ctx.performed)
                jumping = true;
            else if (ctx.canceled)
                jumping = false;
        }

        public override void OnCrunch(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                crunching = true;
            else if (ctx.canceled)
                crunching = false;
        }

        public override void ApplyBraking()
        {
            var brakedForce = rigid.velocity;
            var breakValue = 1f - brakeForce;
            if (rigid.velocity.x != 0)
                brakedForce.x *= breakValue;
            if (rigid.velocity.z != 0)
                brakedForce.z *= breakValue;
            if (rigid.velocity.y < 0)
                brakedForce.y *= breakValue;
            rigid.velocity = brakedForce;
        }
    }
}