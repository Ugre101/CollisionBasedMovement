using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace NavAgentMovement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class PlayerAgentMover : NavAgentMover
    {
        Camera cam;

        protected override void Start()
        {
            base.Start();
            cam = Camera.main;
        }

        public void OnClick(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            var pos = Pointer.current.position.ReadValue();
            var ray = cam.ScreenPointToRay(pos);
            if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, groundLayer)) return;
            if (CalcPath(hit.point))
            {
            }
        }
    }
}