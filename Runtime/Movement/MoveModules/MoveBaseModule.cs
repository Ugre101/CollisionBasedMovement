using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CollsionBasedMovement.MoveModules
{
    [Serializable]
    public abstract class MoveBaseModule
    {
        protected CharacterCapsule capsule;
        protected GroundCheck checker;
        protected Rigidbody rigid;

        public void OnStart(Rigidbody rb, CharacterCapsule cap, GroundCheck gc)
        {
            rigid = rb;
            capsule = cap;
            checker = gc;
        }

        public virtual void OnEnter(Collider collider)
        {
        }

        public virtual void OnExit()
        {
        }

        public virtual void OnUpdate()
        {
        }

        public virtual void OnFixedUpdate()
        {
            OnGravity();
        }

        protected abstract void OnGravity();

        public virtual void OnCapsuleSizeChange(float newHeight)
        {
        }


        public abstract void OnMove(Vector3 force, bool sprinting, float sprintAcc);

        public abstract void OnJump(InputAction.CallbackContext ctx);

        public abstract void OnCrunch(InputAction.CallbackContext ctx);

        public abstract void ApplyBraking();
    }
}