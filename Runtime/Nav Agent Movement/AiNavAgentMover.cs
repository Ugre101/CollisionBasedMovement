using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;

namespace Nav_Agent_Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class AiNavAgentMover : NavAgentMover
    {
        [SerializeField] float moveRange = 6f;

        [Header("Don't change runtime"), SerializeField, Range(1, 12),]
        int batchSize = 6;

        readonly Vector3 offset = new(0, 22, 0);

        bool scheduled;
        NativeArray<RaycastHit> terrainHits;


        JobHandle terrainJobHandle;


        NativeArray<RaycastCommand> terrainTests;

        protected override void Start()
        {
            base.Start();
            terrainTests = new NativeArray<RaycastCommand>(batchSize, Allocator.Persistent);
            terrainHits = new NativeArray<RaycastHit>(batchSize, Allocator.Persistent);
        }

        void Update()
        {
            if (Time.frameCount % 10 != 0)
                return;
            if (agent.remainingDistance > 1f)
                return;
            agent.ResetPath();
            GetDestination();
        }

        void OnDestroy()
        {
            terrainTests.Dispose();
            terrainHits.Dispose();
        }

        void GetDestination()
        {
            if (scheduled)
                CheckSamplePaths();
            else
                ScheduleRaycasts();
        }

        void ScheduleRaycasts()
        {
            for (var i = 0; i < batchSize; i++)
            {
                var testPos = transform.position + offset;
                testPos.z += Random.Range(-moveRange, moveRange);
                testPos.x += Random.Range(-moveRange, moveRange);

                var raycastCommand = new RaycastCommand(testPos, Vector3.down, Mathf.Infinity, groundLayer);
                terrainTests[i] = raycastCommand;
            }

            terrainJobHandle = RaycastCommand.ScheduleBatch(terrainTests, terrainHits, batchSize);
            scheduled = true;
        }

        void CheckSamplePaths()
        {
            scheduled = false;
            terrainJobHandle.Complete();
            for (var i = 0; i < batchSize; i++)
            {
                var hit = terrainHits[i];
                if (hit.collider is null) continue;
                if (!NavMesh.SamplePosition(hit.point, out var nahVit, 1, NavMesh.AllAreas)) continue;
                if (CalcPath(nahVit.position))
                    return;
            }
        }
    }
}