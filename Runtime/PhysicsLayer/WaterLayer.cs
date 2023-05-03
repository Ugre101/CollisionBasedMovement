using System;
using MovementScripts;
using UnityEngine;

namespace PhysicsLayer
{
    public class WaterLayer : BaseLayer
    {
        [SerializeField] Vector3 currentDirection;
        [SerializeField] float currentForce;

        public override void OnEnter(Movement mover)
        {
        }

        public override void OnExit(Movement mover)
        {
        }

        public override void OnFixedUpdate(Movement movement)
        {
        }
    }
}