using CameraScripts;
using CollsionBasedMovement.MoveModules;
using PhysicsLayer;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CollsionBasedMovement
{
    [RequireComponent(typeof(GroundCheck), typeof(Rigidbody), typeof(CharacterCapsule))]
    public class Movement : MoveCharacter
    {
        [SerializeField] CharacterCapsule capsuleCollider;
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
        [SerializeField] MoveWalkingModule walkingModule;
        [SerializeField] MoveHoverModule hoverModule;

        // Max Values
        [SerializeField] float absoluteMaxSpeed = 60f;
        [SerializeField] float absoluteMaxFallSpeed = 40f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float percentSubToSwim = 0.5f;

        bool autoRunning;

        BaseLayer currentLayer;

        MoveBaseModule currentModule;
        Vector3 moveDir;

        int stuckTicks;

        // Inputs
        float y, x;

        void Start()
        {
            Rigid.freezeRotation = true;
            Rigid.useGravity = false;
            walkingModule.OnStart(Rigid, capsuleCollider, groundChecker);
            swimModule.OnStart(Rigid, capsuleCollider, groundChecker);
            hoverModule.OnStart(Rigid, capsuleCollider, groundChecker);
            ChangeModule(walkingModule);
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

        public void Stop()
        {
            if (CurrentMode != MoveModes.Falling)
                Rigid.velocity = Vector3.zero;
        }

        void FixedUpdate()
        {
            groundChecker.OnFixedUpdate();
            Move();
            SpeedControl();
            if (!Moving())
                currentModule.ApplyBraking();
            currentModule.OnFixedUpdate();
            groundChecker.OnEndFixedUpdate();
            //ApplyGravity();
        }

        void OnCollisionEnter(Collision other)
        {
            if (!other.gameObject.TryGetComponent(out BaseLayer baseLayer)) return;
            if (currentLayer != null)
                currentLayer.OnExit(this);
            baseLayer.OnEnter(this);
            currentLayer = baseLayer;
        }

        void OnCollisionExit(Collision other)
        {
            if (!other.gameObject.TryGetComponent(out BaseLayer baseLayer)) return;
            if (baseLayer == currentLayer)
                baseLayer.OnExit(this);
            currentLayer = null;
        }

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
                    ChangeModule(walkingModule);
                    CurrentMode = MoveModes.Hovering;
                    break;
            }
        }

        void StopSwimming()
        {
            ChangeModule(walkingModule);
            CurrentMode = MoveModes.Walking;
            print("Stop Swimming");
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

            var accDelta = MaxSpeed / Mathf.Max(Rigid.velocity.magnitude, 2f);
            var force = moveDir * (Speed * accDelta);
            currentModule.OnMove(force, sprinting, sprintAcceleration);
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
            var flatVel = Rigid.velocity;
            flatVel.y = 0;
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

        public override bool IsCrouching() => CurrentMode == MoveModes.Walking && walkingModule.IsCrunching();

        public override bool IsGrounded() => groundChecker.IsGrounded;
        public override bool WasGrounded() => groundChecker.WasGrounded;

        public override Vector3 GetUpVector() => ori.rotation * Vector3.up;
    }
}