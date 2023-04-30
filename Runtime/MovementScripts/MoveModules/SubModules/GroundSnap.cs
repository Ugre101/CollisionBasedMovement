using System;
using UnityEngine;

namespace MovementScripts.MoveModules.SubModules
{
    [Serializable]
    public class GroundSnap
    {
        [SerializeField, Range(0.01f, 0.5f),] float minDist = 0.1f;
        [SerializeField, Range(0.1f, 1f),] float maxDist = 0.4f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float maxSnapCapsule = 0.1f;

        public bool CheckAndApplyGroundSnap(float capsuleHeight, GroundCheck checker, out Vector3 toSnap)
        {
            toSnap = default;
            var min = capsuleHeight * minDist;
            var max = capsuleHeight * maxDist;
            if (checker.DistanceToGround > max || checker.DistanceToGround < min)
                return false;
            
            var distanceToGround = checker.DistanceToGround;
            var maxSnap = capsuleHeight * maxSnapCapsule;
            toSnap = Vector3.down * Mathf.Min(distanceToGround, maxSnap);
            return true;
        }
    }
}