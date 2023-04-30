using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraScripts
{
    public class ThirdPersonCameraController : CameraSharedBase<Cinemachine3rdPersonFollow>
    {
        [SerializeField] FirstPersonCameraController firstPersonVCamera;
        [SerializeField] Transform cameraOrientation;
        [SerializeField, Range(2f, 6f),] float xSen = 3, ySen = 3;

        [SerializeField, Range(float.Epsilon, 0.05f),]
        float zoomSen = 0.05f;

        [SerializeField, Range(12f, 30f),] float maxZoom = 12f;
        [SerializeField, Range(1f, 2f),] float minZoom = 2f;
        [SerializeField] float maxElevation = 2f;
        [SerializeField] float minElevation = 0.5f;

        [SerializeField, Range(float.Epsilon, 1f),]
        float elevationDelta = 0.2f;

        Vector3 currentElevation;

        Coroutine elevateRoutine;
        Vector2 input;

        bool mouseLocked;

        float xRot, yRot;

        void Start()
        {
            currentElevation = cameraOrientation.transform.localPosition;
        }

        void Update()
        {
            if (!mouseLocked)
                return;
            input *= Time.deltaTime;
            var rot = cameraOrientation.eulerAngles;
            rot.x += input.y * xSen;
            var x = rot.x;
            x = x switch
            {
                <= float.Epsilon => 359.99f,
                > 70f and < 180f => 70f,
                > 70f and < 290f => 290f,
                _ => x,
            };

            rot.x = x;
            rot.y += input.x * ySen;
            rot.z = 0;
            cameraOrientation.rotation = Quaternion.Euler(rot);
        }

        public void Input(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
                input = ctx.ReadValue<Vector2>();
            else if (ctx.canceled)
                input = Vector2.zero;
        }

        public void LockMouse(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                mouseLocked = true;
            }
            else if (ctx.canceled)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                mouseLocked = false;
            }
        }

        public void RotateX(float x)
        {
        }

        public override void ZoomInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            var change = ctx.ReadValue<float>();
            ComponentBase.CameraDistance =
                Mathf.Clamp(ComponentBase.CameraDistance - change * zoomSen, minZoom, maxZoom);
            if (ComponentBase.CameraDistance <= minZoom && change > 0)
                SwitchToFirstPerson();
        }

        void SwitchToFirstPerson()
        {
            gameObject.SetActive(false);
            firstPersonVCamera.SwitchTo();
        }

        public void ElevateCamera(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                var elevate = ctx.ReadValue<float>();
                if (elevateRoutine != null)
                    StopCoroutine(elevateRoutine);
                elevateRoutine = StartCoroutine(ElevateCam(elevate));
            }
            else if (ctx.canceled && elevateRoutine != null)
            {
                StopCoroutine(elevateRoutine);
            }
        }

        IEnumerator ElevateCam(float elevate)
        {
            while (true)
            {
                var y = cameraOrientation.transform.localPosition.y;
                currentElevation.y = Mathf.Clamp(y + elevate * elevationDelta * Time.deltaTime, minElevation,
                    maxElevation);
                cameraOrientation.transform.localPosition = currentElevation;
                if (y >= maxElevation && y <= minElevation)
                    yield break;
                yield return new WaitForEndOfFrame();
            }
        }

        public void SwitchTo()
        {
            cameraOrientation.rotation = mainCamera.rotation;
            gameObject.SetActive(true);
            CameraSettings.SetCameraMode(CameraSettings.CameraMode.ThirdPerson);
        }
    }
}