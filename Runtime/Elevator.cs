using System.Collections;
using MovementScripts;
using PhysicsLayer;
using UnityEngine;

namespace DefaultNamespace
{
    public class Elevator : MovingLayer
    {
        [SerializeField] Vector3 direction = Vector3.up;
        [SerializeField] float distance = 2f;
        [SerializeField] float speed = 1f;

        Vector3 startPos;
        Vector3 target;
        void Start()
        {
            startPos = transform.position;
            target = startPos + direction * distance;
        }

        bool towards = true;

        public override void OnFixedUpdate(Movement movement)
        {
            if (towards)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
                if (ReachedDestination(target)) 
                    towards = false;
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
                if (ReachedDestination(startPos)) 
                    towards = true;
            }
        }

        bool ReachedDestination(Vector3 dest) => (transform.position - dest).magnitude < float.Epsilon;

        public override void OnEnter(Movement mover)
        {
            base.OnEnter(mover);
            if (returnRoutine != null)
                StopCoroutine(returnRoutine);
        }

        Coroutine returnRoutine;
        public override void OnExit(Movement mover)
        {
            base.OnExit(mover);
            if (ReachedDestination(startPos) is false)
                returnRoutine =  StartCoroutine(ReturnToStart());
        }

        IEnumerator ReturnToStart()
        {
            while (ReachedDestination(startPos) is false)
            {
                transform.position = Vector3.MoveTowards(transform.position, startPos, speed * Time.deltaTime);
                yield return null;
            }
        }
    }
}