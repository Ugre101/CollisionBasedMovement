using System;
using AvatarScripts;
using MovementScripts.MoveModules.SubModules;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementScripts.MoveModules
{
    [Serializable]
    public class MoveWalkingModule : MoveBaseModule
    {
        [SerializeField, Range(float.Epsilon, 1f),]
        float airMultiplier = 0.4f;

        [SerializeField, Range(1f, 3f),] float acceleration = 1.5f;
        [SerializeField] GroundJumping groundJumping;
        [SerializeField] GroundSnap groundSnap;
        [SerializeField] ForceMode forceMode = ForceMode.Force;
        [SerializeField, Range(0f, 0.2f),] float brakeForce = 0.2f;
        bool crunching;

        bool wantToUnCrunch;
        bool wantToCrunch;
        public bool IsJumping => groundJumping.Jumping;

        public override void OnStart(Rigidbody rb, CharacterCapsule cap, GroundCheck gc, MoveStats ms)
        {
            base.OnStart(rb, cap, gc, ms);
            groundJumping.OnStart(ms);
        }

        protected override void OnGravity()
        {
            if (checker.OnSlope())
                SlopeGravity(rigid);
            else
                rigid.AddForce(Physics.gravity, ForceMode.Force);
        }

        public override void OnExit()
        {
            base.OnExit();
            if (crunching)
                StopCrunching();
        }

        public override void OnMove(Vector3 force, bool sprinting, float sprintAcc)
        {
            force *= checker.IsGrounded ? acceleration : airMultiplier;
            if (checker.IsGrounded && sprinting)
                force *= sprintAcc;


            switch (checker.CurrentGroundState)
            {
                case GroundCheck.GroundState.Falling when groundJumping.Jumping is false:
                    if (groundSnap.CheckAndApplyGroundSnap(capsule.Height, checker, out var toSnap))
                    {
                        if (rigid.velocity.y > 0)
                        {
                            var vel = rigid.velocity;
                            vel.y = 0;
                            rigid.velocity = vel;
                        }
                        rigid.position += toSnap;
                    }
                    break;
                case GroundCheck.GroundState.Falling when checker.Colliding:
                {
                    var slide = Vector3.ProjectOnPlane(new Vector3(0, rigid.velocity.y, 0), Vector3.down);
                    force = slide;
                    break;
                }
                case GroundCheck.GroundState.Sliding:
                {
                    var slide = Vector3.ProjectOnPlane(new Vector3(0, rigid.velocity.y, 0), Vector3.down);
                    force = slide;
                    break;
                }
                case GroundCheck.GroundState.Flat or GroundCheck.GroundState.Incline:
                {
                    if (checker.CanIStepUp(out var toMove))
                        rigid.position += toMove;
                    else if (checker.CheckStuck(out var toPush))
                        rigid.position += toPush;
                    break;
                }
            }

            rigid.AddForce(force, forceMode);
        }

        public override void OnUpdate()
        {
            groundJumping.OnUpdate(checker);
            if (wantToCrunch && checker.IsGrounded) 
                StartCrunching();
            if (wantToUnCrunch && CanStopCrunching()) 
                StopCrunching();
        }

        void StartCrunching()
        {
            wantToCrunch = false;
            crunching = true;
            var c1 = capsule.YMin;
            capsule.HalfHeight();
            var c2 = capsule.YMin;
            rigid.position += Vector3.down * (c2 - c1);
            StartedCrunching?.Invoke();
        }

        public event Action StartedCrunching;
        public event Action StoppedCrounching;
        bool CanStopCrunching()
        {
            var capPos = capsule.Center;
            var offset = capsule.Height / 10;
            capPos.y -= offset;
            var ray = new Ray(capPos, Vector3.up);
            return !Physics.SphereCast(ray, capsule.Radius, capsule.Height);
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            groundJumping.OnFixedUpdate(rigid);
        }

        public override void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                groundJumping.WantToJump(true);
            else if (ctx.canceled)
                groundJumping.WantToJump(false);
        }

        public override void OnCrunch(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && !crunching)
            {
                wantToCrunch = true;
                wantToUnCrunch = false;
            }
            else if (ctx.canceled)
            {
                wantToUnCrunch = true;
                wantToCrunch = false;
            }
        }

        public override void ApplyBraking()
        {
            if (!checker.IsGrounded)
                return;
            var brakedForce = rigid.velocity;
            var breakValue = 1f - brakeForce;
            if (rigid.velocity.x != 0)
                brakedForce.x *= breakValue;
            if (rigid.velocity.z != 0)
                brakedForce.z *= breakValue;
            if (rigid.velocity.y > 0 && checker.OnSlope())
                brakedForce.y *= breakValue;
            rigid.velocity = brakedForce;
        }

        void StopCrunching()
        {
            crunching = false;
            wantToUnCrunch = false;
            capsule.RestoreHeight();
            StoppedCrounching?.Invoke();
        }

        static void SlopeGravity(Rigidbody rb)
        {
            if (rb.velocity.y < 0)
                return;
            rb.AddForce(Physics.gravity, ForceMode.Force);
        }

        public bool IsCrunching() => crunching;
    }
}