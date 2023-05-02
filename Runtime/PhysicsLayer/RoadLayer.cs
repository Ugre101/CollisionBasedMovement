using System;
using System.Collections;
using MovementScripts;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PhysicsLayer
{
    public class RoadLayer : BaseLayer
    {
        [SerializeField, Range(0.5f, 3f),] float removeDelay = 1f;
        [SerializeField] FloatMod speedMod;

        Coroutine removeRoutine;

        WaitForSeconds waitForSeconds;
        void Start() => waitForSeconds = new WaitForSeconds(removeDelay);

        public override void OnEnter(Movement movement)
        {
            if (removeRoutine is not null)
                StopCoroutine(removeRoutine);
            else
                movement.Stats.AddMod(MoveCharacter.MoveModes.Walking, speedMod);
        }

        public override void OnExit(Movement movement)
        {
            removeRoutine = StartCoroutine(RemoveAfterDelay(movement.Stats));
        }

        public override void OnFixedUpdate(Movement movement)
        {
        }

        IEnumerator RemoveAfterDelay(MoveStats moveStatsManager)
        {
            yield return waitForSeconds;
            moveStatsManager.RemoveMod(MoveCharacter.MoveModes.Walking, speedMod);
            removeRoutine = null;
        }
    }
}