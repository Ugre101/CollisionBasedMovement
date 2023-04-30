using System;
using UnityEngine;
using UnityEngine.Events;

namespace CollsionBasedMovement
{
    [Serializable]
    public class GroundJumping
    {
        [SerializeField] float jumpForce = 1f;
        [SerializeField] float jumpCooldown = 0.2f;
        [SerializeField, Range(1, 7),] int maxJumps = 2;

        public UnityEvent jumped;
        bool coolDownPassed = true;
        int jumpCount;

        float lastJump;
        bool wantToJump;

        public bool Jumping { get; private set; }

        public void CheckCoolDownPassed()
        {
            if (coolDownPassed)
                return;
            coolDownPassed = lastJump + jumpCooldown < Time.time;
        }

        bool CanJump() => wantToJump && jumpCount < maxJumps && coolDownPassed;

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
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            lastJump = Time.time;
            jumped?.Invoke();
        }

        public void WantToJump(bool value) => wantToJump = value;
    }
}