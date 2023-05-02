using System.Collections.Generic;
using AvatarScripts;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MovementScripts
{
    public class GroundCheck : MonoBehaviour
    {
        public enum GroundState
        {
            Falling,
            Flat,
            Incline,
            Sliding,
        }

        const float MaxDistanceToGround = 100f;

        [SerializeField] LayerMask groundLayer;
        [SerializeField] CharacterCapsule capsule;
        [SerializeField] Rigidbody rigid;
        [SerializeField, Range(0f, 90f),] float maxSlope = 45f;

        [SerializeField] StairChecker stairChecker = new();
        [SerializeField] float stuckPushDelta = 0.1f;

        [SerializeField, Range(0.001f, 0.4f),] float coyoteTime = 0.1f;
        readonly HashSet<ContactPoint> contactPoints = new();
        float coyoteTick;

        Vector3 dir;

        ContactPoint ground;
        StairChecker.StepInputData inputData;

        NativeArray<SpherecastCommand> spherecastCommands;

        JobHandle sphereCastHandle;

        NativeArray<RaycastHit> spherecastHits;

        public Vector3 GroundNormal => ground.normal;
        public bool IsGrounded { get; private set; }
        public float DistanceToGround { get; private set; }
        public RaycastHit DistanceToGroundHit { get; private set; }
        public GroundState CurrentGroundState { get; private set; }
        public bool Colliding { get; private set; }
        public bool WasGrounded { get; private set; }

        void Start()
        {
            spherecastCommands = new NativeArray<SpherecastCommand>(1, Allocator.Persistent);
            spherecastHits = new NativeArray<RaycastHit>(1, Allocator.Persistent);

            inputData = new StairChecker.StepInputData(capsule, default, default, maxSlope, default, groundLayer);
        }

        void OnDestroy()
        {
            spherecastCommands.Dispose();
            spherecastHits.Dispose();
        }

        void OnCollisionEnter(Collision collision) => contactPoints.UnionWith(collision.contacts);

        void OnCollisionStay(Collision collision) => contactPoints.UnionWith(collision.contacts);
#if UNITY_EDITOR
        void OnValidate()
        {
            if (Application.isPlaying)
                return;
            if (capsule == null && !TryGetComponent(out capsule))
                throw new MissingComponentException();
            if (rigid == null && !TryGetComponent(out rigid))
                throw new MissingComponentException();
        }
#endif

        void CheckDist()
        {
            sphereCastHandle.Complete();


            var hit = spherecastHits[0];
            if (hit.collider is not null)
            {
                DistanceToGround = hit.distance - capsule.Height / 2f + capsule.Radius;
                DistanceToGroundHit = hit;
            }
            else
            {
                DistanceToGround = MaxDistanceToGround;
            }

            var raycastCommand = new SpherecastCommand(capsule.Center, capsule.Radius, Vector3.down,
                MaxDistanceToGround, groundLayer);
            spherecastCommands[0] = raycastCommand;

            sphereCastHandle = SpherecastCommand.ScheduleBatch(spherecastCommands, spherecastHits, 1);
        }

        public void OnFixedUpdate()
        {
            IsGrounded = CheckGrounded(contactPoints, out ground);
            HandleCoyoteTime();

            Colliding = contactPoints.Count > 0;
            CheckDist();
        }

        void HandleCoyoteTime()
        {
            if (IsGrounded)
            {
                WasGrounded = true;
                coyoteTick = 0;
            }
            else if (coyoteTick < coyoteTime)
            {
                coyoteTick += Time.deltaTime;
            }
            else if (WasGrounded)
            {
                WasGrounded = false;
            }
        }

        bool CheckGrounded(HashSet<ContactPoint> list, out ContactPoint groundPoint)
        {
            groundPoint = default;
            var found = false;
            if (list.Count <= 0)
            {
                CurrentGroundState = GroundState.Falling;
                return false;
            }


            foreach (var point in list)
                if (point.normal.y > float.Epsilon && (!found || groundPoint.normal.y < point.normal.y))
                {
                    groundPoint = point;
                    found = true;
                }


            if (!found)
            {
                CurrentGroundState = GroundState.Falling;
                return false;
            }

            var angle = Vector3.Angle(groundPoint.normal, Vector3.up);
            if (angle == 0)
                CurrentGroundState = GroundState.Flat;
            else if (angle < maxSlope)
                CurrentGroundState = GroundState.Incline;
            else
                CurrentGroundState = GroundState.Sliding;

            return angle < maxSlope;
        }


        public bool CheckStuck(out Vector3 pushDir)
        {
            pushDir = Vector3.zero;
            if (contactPoints.Count == 0)
                return false;
            if (IsGrounded && contactPoints.Count == 1)
                return false;

            foreach (var point in contactPoints)
            {
                if (point.separation > 0)
                    continue;
                pushDir += point.normal;
            }

            if (pushDir == Vector3.zero)
                return false;

            pushDir = SlopeDir(pushDir) * stuckPushDelta;
            return true;
        }

        public void OnEndFixedUpdate() => contactPoints.Clear();

        public bool CanIStepUp(out Vector3 toMove)
        {
            toMove = Vector3.zero;
            if (!IsGrounded || dir.magnitude < 0)
                return false;
            if (contactPoints.Count <= 1)
                return false;
            inputData.Update(ground.point.y, SlopeAngle(), dir);
            return stairChecker.StepCheck(contactPoints, out toMove, inputData);
        }

        public bool OnSlope()
        {
            if (IsGrounded is false)
                return false;
            var angle = SlopeAngle();
            return angle > float.Epsilon && angle < maxSlope;
        }

        float SlopeAngle() => Vector3.Angle(ground.normal, Vector3.up);

        public Vector3 SlopeDir(Vector3 moveDir)
            => Vector3.ProjectOnPlane(moveDir, ground.normal).normalized;

        public void SetDirection(Vector3 force) => dir = force;
    }
}