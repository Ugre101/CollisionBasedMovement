using System;
using MovementScripts;
using UnityEngine;
using UnityEngine.AI;

namespace Nav_Agent_Movement
{
    public class NavAgentMover : MoveCharacter
    {
        [SerializeField] protected NavMeshAgent agent;
        [SerializeField] protected LayerMask groundLayer;
        protected NavMeshPath newPath;
        protected float startOffset;

        protected virtual void Start()
        {
            startOffset = agent.baseOffset;
            newPath = new NavMeshPath();
        }

        void OnTriggerEnter(Collider other)
        {
            if (false && other.gameObject.layer == LayerMask.NameToLayer("Water")) agent.agentTypeID = -334000983;
        }

        void OnTriggerExit(Collider other)
        {
            if (false && other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                agent.baseOffset = startOffset;
                agent.agentTypeID = 0;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (false && other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                var positionY = other.bounds.max.y - transform.position.y;
                if (positionY < 1f)
                {
                }

                var yDiff = positionY + OffDiff();
                var agentBaseOffset = startOffset + yDiff - agent.height;
                agent.baseOffset = agentBaseOffset;
                if (gameObject.CompareTag("Player"))
                {
                    print($"Step 1: {other.bounds.max.y} - {transform.position.y}");
                    print($"Step 2: {positionY} + {OffDiff()}");
                    print($"Final : {startOffset} + {yDiff} - {agent.height / 2f}");
                }
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (agent == null && !TryGetComponent(out agent))
                throw new MissingComponentException();
        }
#endif

        public override bool IsCrouching() => false;

        public override bool IsGrounded() => true;

        public override bool WasGrounded() => true;

        public override Vector3 GetLocalMoveDirection() => throw new NotImplementedException();

        public override Vector3 GetUpVector() => throw new NotImplementedException();
        public override bool IsJumping() => false;

        float OffDiff()
        {
            var offDiff = agent.baseOffset - startOffset;
            return offDiff;
        }

        protected bool CalcPath(Vector3 pos)
        {
            if (!agent.CalculatePath(pos, newPath)) return false;
            if (newPath.status != NavMeshPathStatus.PathComplete) return false;
            agent.SetPath(newPath);
            return true;
        }
    }
}