using UnityEngine;

namespace AvatarScripts
{
    public class ActorOrientation : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform actorTransform;

        void Update()
        {
            if (rb.velocity.magnitude == 0) return;
            UpdateRotation();
        }

        public void SetActorTransform(Transform trans) => actorTransform = trans;

        public void UpdateRotation()
        {
            actorTransform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
        }
    }
}