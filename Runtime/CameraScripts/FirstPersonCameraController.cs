using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraScripts
{
    public class FirstPersonCameraController : CameraSharedBase<CinemachinePOV>
    {
        [SerializeField] ThirdPersonCameraController thirdPersonCamera;

        public void UpdateXValue(float value)
        {
            ComponentBase.m_HorizontalAxis.Value = value;
        }

        public override void ZoomInput(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            var change = ctx.ReadValue<float>();
            if (change < 0)
                SwitchToFirstPerson();
        }

        public void SwitchTo()
        {
            ComponentBase.ForceCameraPosition(Vector3.zero, mainCamera.rotation);
            gameObject.SetActive(true);
            CameraSettings.SetCameraMode(CameraSettings.CameraMode.FirstPerson);
        }

        void SwitchToFirstPerson()
        {
            gameObject.SetActive(false);
            thirdPersonCamera.SwitchTo();
        }
    }
}