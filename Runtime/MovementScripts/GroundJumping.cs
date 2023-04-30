using System;
using UnityEngine;
using UnityEngine.Events;

namespace MovementScripts
{
    [Serializable]
    public class GroundJumping
    {
        [SerializeField] float jumpCooldown = 0.2f;

        public UnityEvent jumped;
        bool coolDownPassed = true;
        int jumpCount;

        float lastJump;
        MoveStats stats;
        bool wantToJump;

        public bool Jumping { get; private set; }

        public void CheckCoolDownPassed()
        {
            if (coolDownPassed)
                return;
            coolDownPassed = lastJump + jumpCooldown < Time.time;
        }

        bool CanJump() => wantToJump && jumpCount < stats.MaxJumpCount && coolDownPassed;

        public void OnUpdate(GroundCheck checker)
        {
            CheckCoolDownPassed();
            CheckResetJumpCount(checker);
        }

        void CheckResetJumpCount(GroundCheck groundChecker)
        {
            if (!coolDownPassed || jumpCount <= 0 || !groundChecker.IsGrounded)
                return;
            jumpCount = 0;
            Jumping = false;
        }

        public void OnFixedUpdate(Rigidbody rb)
        {
            if (!CanJump())
                return;
            Jumping = true;
            jumpCount++;
            coolDownPassed = false;
            if (rb.velocity.y < 0)
            {
                var vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;
            }

            rb.AddForce(Vector3.up * stats.JumpStrength, ForceMode.Impulse);
            lastJump = Time.time;
            jumped?.Invoke();
        }

        public void WantToJump(bool value) => wantToJump = value;

        public void OnStart(MoveStats ms)
        {
            stats = ms;
        }
    }
}