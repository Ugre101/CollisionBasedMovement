using System;
using System.Collections.Generic;
using AvatarScripts;
using UnityEngine;

namespace MovementScripts
{
    [Serializable]
    public class StairChecker
    {
        [SerializeField, Range(0f, 2f),] float maxStepHeight = 0.3f;
        [SerializeField, Range(0f, 1f),] float minStepHeight = 0.3f;
        [SerializeField, Range(0.1f, 0.4f),] float stepSearchOvershoot = 0.2f;
        [SerializeField] bool progressiveNarrowStep;

        Ray castStepRay = new(Vector3.zero, Vector3.down);
        Vector3 origin;

        float overShootValue;
        Collider[] results = new Collider[32];


        Vector3 stepTestInvDir;

        public bool StepCheck(HashSet<ContactPoint> hashSet, out Vector3 stepUpOffset, StepInputData inputData)
        {
            stepUpOffset = default;
            overShootValue = stepSearchOvershoot;
            foreach (var point in hashSet)
                if (CanStep(point, out var hit, inputData))
                {
                    CalcHit(out stepUpOffset, point, hit.y, inputData.GroundY);
                    return true;
                }

            if (!progressiveNarrowStep) return false;
            overShootValue /= 2f;
            foreach (var point in hashSet)
                if (CanStep(point, out var hit, inputData))
                {
                    CalcHit(out stepUpOffset, point, hit.y, inputData.GroundY);
                    return true;
                }

            return false;
        }

        bool CanStep(ContactPoint point, out Vector3 stepHit, StepInputData inputData)
        {
            stepHit = default;

            var yDiff = point.point.y - inputData.GroundY;
            var maxStep = maxStepHeight * inputData.Capsule.Height;
            if (maxStep < yDiff) return false;

            var angle = Vector3.Angle(point.normal, Vector3.up);
            if (angle < float.Epsilon) return false;

            var angleDiff = angle - inputData.SlopeAngle;
            if (angleDiff < inputData.MaxSlopeAngle) return false;

            var thisCenter = point.thisCollider.bounds.center;
            var otherCenter = point.otherCollider.bounds.center;
            var dist = thisCenter - otherCenter;
            var distDir = thisCenter + inputData.Direction - otherCenter;

            if (dist.magnitude < distDir.magnitude)
                //TODO test thoroughly
                return false;


            if (!RayCastStep(point, out var hitInfo, inputData.MaxSlopeAngle, maxStep))
                return false;

            stepHit = hitInfo.point;
            var capsuleBottom = hitInfo.point;
            capsuleBottom.y += 0.01f;
            var size = inputData.Capsule.OverlapCapsuleNonAlloc(capsuleBottom, results, inputData.GroundLayer);
            if (size <= 0) return true;
            for (var index = 0; index < size; index++)
                if (results[index].CompareTag("Player") is false)
                    return false;

            return true;
        }

        bool RayCastStep(ContactPoint point, out RaycastHit hitInfo, float maxSlope, float maxStep)
        {
            var stepHeight = point.point.y + maxStep + Physics.defaultContactOffset;
            castStepRay.origin = GetOrigin(point, stepHeight);
            if (!point.otherCollider.Raycast(castStepRay, out hitInfo, maxStep + 0.2f))
                return false;

            return !(Vector3.Angle(hitInfo.normal, Vector3.up) > maxSlope);
        }

        Vector3 GetOrigin(ContactPoint point, float stepHeight)
        {
            stepTestInvDir.Set(-point.normal.x, 0, -point.normal.z);
            origin.Set(point.point.x, stepHeight, point.point.z);
            origin += stepTestInvDir.normalized * overShootValue;
            return origin;
        }


        void CalcHit(out Vector3 stepUpOffset, ContactPoint point, float hitY, float pointY) =>
            stepUpOffset = GetOrigin(point, hitY) - new Vector3(point.point.x, pointY, point.point.z);

        public struct StepInputData
        {
            public StepInputData(CharacterCapsule capsule, float groundY, float slopeAngle, float maxSlopeAngle,
                                 Vector3 direction, LayerMask groundLayer)
            {
                Capsule = capsule;
                GroundY = groundY;
                SlopeAngle = slopeAngle;
                MaxSlopeAngle = maxSlopeAngle;
                Direction = direction;
                GroundLayer = groundLayer;
            }

            public void Update(float groundY, float slopeAngle, Vector3 direction)
            {
                GroundY = groundY;
                SlopeAngle = slopeAngle;
                Direction = direction;
            }

            public CharacterCapsule Capsule { get; }
            public float GroundY { get; private set; }
            public float SlopeAngle { get; private set; }
            public Vector3 Direction { get; private set; }
            public float MaxSlopeAngle { get; }
            public LayerMask GroundLayer { get; }
        }
    }
}