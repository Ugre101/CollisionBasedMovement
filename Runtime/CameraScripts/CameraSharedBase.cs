using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CameraScripts
{
    public abstract class CameraSharedBase<T> : MonoBehaviour where T : CinemachineComponentBase
    {
        [SerializeField] protected CinemachineVirtualCamera virtualCamera;
        [SerializeField] protected Transform mainCamera;
        T componentBase;

        bool gotBaseComponent;

        protected T ComponentBase
        {
            get
            {
                if (gotBaseComponent)
                    return componentBase;
                componentBase = virtualCamera.GetCinemachineComponent<T>();
                if (componentBase == null)
                    throw new MissingComponentException();
                return componentBase;
            }
        }

        public abstract void ZoomInput(InputAction.CallbackContext ctx);
    }

    public static class CameraSettings
    {
        public enum CameraMode
        {
            ThirdPerson,
            FirstPerson,
        }

        public static CameraMode CurrentMode { get; private set; } = CameraMode.ThirdPerson;

        public static void SetCameraMode(CameraMode newMode)
        {
            CurrentMode = newMode;
        }
    }
}