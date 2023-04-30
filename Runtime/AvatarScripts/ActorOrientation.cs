using System;
using UnityEngine;

namespace AvatarScripts
{
    public class ActorOrientation : MonoBehaviour
    {
        [SerializeField] Rigidbody rb;
        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform actorTransform;


        bool hasActor;

        void Update()
        {
            if (hasActor is false)
                return;
            if (rb.velocity.magnitude == 0) 
                return;
            UpdateRotation();
        }

        
        public void SetActorTransform(Transform trans)
        {
            actorTransform = trans;
            hasActor = true;
        }

        public void SetActorWithAnimator(Animator trans) => actorTransform = trans.transform;

        void UpdateRotation() => actorTransform.rotation = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0);
    }
}