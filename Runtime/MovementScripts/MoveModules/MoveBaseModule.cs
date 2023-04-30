using System;
using AvatarScripts;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementScripts.MoveModules
{
    [Serializable]
    public abstract class MoveBaseModule
    {
        protected CharacterCapsule capsule;
        protected GroundCheck checker;
        protected Rigidbody rigid;
        protected MoveStats stats;
        public virtual void OnStart(Rigidbody rb, CharacterCapsule cap, GroundCheck gc,MoveStats ms)
        {
            rigid = rb;
            capsule = cap;
            checker = gc;
            stats = ms;
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