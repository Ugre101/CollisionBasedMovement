using System;
using AvatarScripts;
using CameraScripts;
using MovementScripts.MoveModules;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace MovementScripts
{
    [RequireComponent(typeof(GroundCheck), typeof(Rigidbody), typeof(CharacterCapsule))]
    public class Movement : MoveCharacter
    {
        [SerializeField] Transform ori;
        [SerializeField] GroundCheck groundChecker;


        // Camera
        [Header("Camera Variables"), SerializeField,]
        Transform mainCamera;

        // old 250fps ish


        [SerializeField] Transform cameraTarget;
        [SerializeField] bool turnOnX;

        // Ground and Air Drag
        [Header("Drag Variables"), SerializeField, Range(float.Epsilon, 2f),]
        float groundDrag = 0.5f;

        // Sprint

        // Modules 
        [SerializeField] MoveSwimModule swimModule;
        [SerializeField] MoveHoverModule hoverModule;

        // Max Values
        [SerializeField] float absoluteMaxSpeed = 60f;
        [SerializeField] float absoluteMaxFallSpeed = 40f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float percentSubToSwim = 0.5f;

        [SerializeField, Range(1f, 10f),] float directionChangeBoost = 2f;

        bool autoRunning;

        MoveBaseModule currentModule;
        Vector3 moveDir;

        int stuckTicks;

        // Inputs
        float y, x;
        [SerializeField,Range(float.Epsilon,1f),] float directionChangeThreshold = 0.2f;
        [SerializeField, Range(2f,10f)] float maxLowVelBoost = 4f;
        [field: SerializeField] public MoveWalkingModule WalkingModule { get; private set; }

        PhysicLayerHandler PhysicLayerHandler { get; } = new();

        void Start()
        {
            Rigid.freezeRotation = true;
            Rigid.useGravity = false;
            WalkingModule.OnStart(Rigid, capsuleCollider, groundChecker, Stats);
            swimModule.OnStart(Rigid, capsuleCollider, groundChecker, Stats);
            hoverModule.OnStart(Rigid, capsuleCollider, groundChecker, Stats);
            ChangeModule(WalkingModule);
            transform.SetParent(null);
        }

        void Update()
        {
            var drag = groundDrag;
            if (groundChecker.OnSlope() && !Moving())
                drag *= 3f;
            Rigid.drag = groundChecker.IsGrounded ? drag : 0f;


            CurrentMode = CurrentMode switch
            {
                MoveModes.Walking when !groundChecker.IsGrounded => MoveModes.Falling,
                MoveModes.Falling when groundChecker.IsGrounded => MoveModes.Walking,
                _ => CurrentMode,
            };

            currentModule.OnUpdate();
        }

        void FixedUpdate()
        {
            groundChecker.OnFixedUpdate();
            Move();
            SpeedControl();
            if (!Moving())
                currentModule.ApplyBraking();
            currentModule.OnFixedUpdate();
            PhysicLayerHandler.OnFixedUpdate(this);
            groundChecker.OnEndFixedUpdate();
            //ApplyGravity();
        }

        void OnCollisionEnter(Collision other) => PhysicLayerHandler.TryEnterPhysicsLayer(this, other);


        void OnCollisionExit(Collision other) => PhysicLayerHandler.TryExitPhysicsLayer(this, other);

        void OnTriggerEnter(Collider other)
        {
            ShouldISwim(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (Swimming && other.gameObject.layer == LayerMask.NameToLayer("Water"))
                StopSwimming();
        }

        void OnTriggerStay(Collider other) => ShouldISwim(other);

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (mainCamera == null && Camera.main != null)
                mainCamera = Camera.main.transform;

            if (capsuleCollider == null && !TryGetComponent(out capsuleCollider))
                throw new MissingComponentException("Missing capsule collider");

            if (groundChecker == null && TryGetComponent(out groundChecker))
                throw new MissingComponentException("Missing ground checker");
        }
#endif

        public void Stop()
        {
            if (CurrentMode != MoveModes.Falling)
                Rigid.velocity = Vector3.zero;
        }

        void ShouldISwim(Collider other)
        {
            if (other.gameObject.layer != LayerMask.NameToLayer("Water"))
                return;
            var aboveWater = capsuleCollider.YMax - other.bounds.max.y;
            if (aboveWater <= 0)
            {
                StartSwimming(other);
                return;
            }

            var percentAbove = aboveWater / capsuleCollider.Height;
            var shouldSwim = 1f - percentAbove > percentSubToSwim;
            if (shouldSwim)
            {
                StartSwimming(other);
                return;
            }

            if (Swimming)
                StopSwimming();
        }

        [ContextMenu("Start Hovering")]
        public void ToggleHovering()
        {
            switch (CurrentMode)
            {
                case MoveModes.Walking or MoveModes.Falling:
                    ChangeModule(hoverModule);
                    CurrentMode = MoveModes.Hovering;
                    break;
                case MoveModes.Hovering:
                    ChangeModule(WalkingModule);
                    CurrentMode = MoveModes.Hovering;
                    break;
            }
        }

        void StopSwimming()
        {
            ChangeModule(WalkingModule);
            CurrentMode = MoveModes.Walking;
        }

        void StartSwimming(Collider other)
        {
            ChangeModule(swimModule, other);
            CurrentMode = MoveModes.Swimming;
        }

        void ChangeModule(MoveBaseModule newModule, Collider coll = null)
        {
            currentModule?.OnExit();
            currentModule = newModule;
            currentModule.OnEnter(coll);
        }

        public void OnJump(InputAction.CallbackContext ctx) => currentModule.OnJump(ctx);

        public void OnCrunch(InputAction.CallbackContext ctx) => currentModule.OnCrunch(ctx);

        public void Input(InputAction.CallbackContext ctx)
        {
            if (ctx.started || ctx.performed)
            {
                var v2 = ctx.ReadValue<Vector2>();
                y = v2.y;
                x = v2.x;
                autoRunning = false;
            }
            else if (ctx.canceled)
            {
                y = 0;
                x = 0;
            }
        }

        public void AutoRun(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;
            autoRunning = !autoRunning;
            y = autoRunning ? 1f : 0f;
        }


        bool Moving() => x != 0 || y != 0;

        public void Sprint(InputAction.CallbackContext ctx)
        {
            if (ctx.started || ctx.performed)
                sprinting = true;
            else if (ctx.canceled)
                sprinting = false;
        }

        void Move()
        {
            AlignWithCamera();
            var inputDir = turnOnX && CameraSettings.CurrentMode == CameraSettings.CameraMode.ThirdPerson
                ? ThirdPersonTurn()
                : ori.forward * y + ori.right * x;

            moveDir = groundChecker.OnSlope() ? groundChecker.SlopeDir(inputDir) : inputDir.normalized;
            groundChecker.SetDirection(moveDir);

            var force = moveDir;
            if (Rigid.velocity.magnitude is < 7f and > float.Epsilon)
                force *= Mathf.Min(MaxSpeed / Rigid.velocity.magnitude,maxLowVelBoost);

            force = DirectionChangeBoost(force);
            currentModule.OnMove(force, sprinting, sprintAcceleration);
        }

        Vector3 DirectionChangeBoost(Vector3 force)
        {
            var dirDiff = (FlatVel(Rigid.velocity).normalized - FlatVel(force).normalized).magnitude;
            if (dirDiff > directionChangeThreshold)
                force *= directionChangeBoost;
            return force;
        }


        static Vector3 FlatVel(Vector3 vel)
        {
            var flatVel = vel;
            flatVel.Set(vel.x, 0, vel.z);
            return flatVel;
        }

        Vector3 ThirdPersonTurn()
        {
            var euler = cameraTarget.eulerAngles;
            euler.y += x;
            euler.z = 0;
            cameraTarget.rotation = Quaternion.Euler(euler);
            return ori.forward * y;
        }

        void AlignWithCamera()
        {
            if (y == 0) return;
            var cameraRot = ori.eulerAngles;
            cameraRot.y = mainCamera.eulerAngles.y;
            ori.rotation = Quaternion.Euler(cameraRot);
        }

        void SpeedControl()
        {
            var flatVel = FlatVel(Rigid.velocity);
            if (flatVel.magnitude < MaxSpeed)
                return;
            if (MaxSpeed > absoluteMaxSpeed)
            {
                // TODO
            }

            var limitVel = flatVel.normalized * MaxSpeed;
            if (Rigid.velocity.y > absoluteMaxFallSpeed)
                limitVel.y = absoluteMaxFallSpeed;
            else if (Rigid.velocity.y < -absoluteMaxSpeed)
                limitVel.y = -absoluteMaxFallSpeed;
            else
                limitVel.y = Rigid.velocity.y;
            Rigid.velocity = limitVel;
        }

        public override Vector3 GetLocalMoveDirection() => ori.InverseTransformDirection(moveDir);

        public override bool IsCrouching() => CurrentMode == MoveModes.Walking && WalkingModule.IsCrunching();

        public override bool IsGrounded() => groundChecker.IsGrounded;
        public override bool WasGrounded() => groundChecker.WasGrounded;

        public override bool IsJumping() => WalkingModule.IsJumping;

        public override Vector3 GetUpVector() => ori.rotation * Vector3.up;

        public void AddMod(MoveModes walking, FloatMod speedMod)
        {
            throw new NotImplementedException();
        }

        public void StartClimbing()
        {
            throw new NotImplementedException();
        }

        public void StopClimbing()
        {
            throw new NotImplementedException();
        }
    }
}