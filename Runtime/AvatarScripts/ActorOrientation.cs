using System;
using UnityEngine;

namespace AvatarScripts
{
    public class ActorOrientation : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform actorTransform;


        bool actorInNull = true;


        void Start()
        {
            actorInNull = actorTransform == null;
        }

        void Update()
        {
            if (actorInNull)
                return;
            if (rb.velocity.magnitude == 0) return;
            UpdateRotation();
        }

        
        public void SetActorTransform(Transform trans)
        {
            actorTransform = trans;
            actorInNull = false;
        }

        public void SetActorTransform(Animator trans) => actorTransform = trans.transform;

        void UpdateRotation() => actorTransform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
    }
}