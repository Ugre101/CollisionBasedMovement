using MovementScripts;

namespace PhysicsLayer
{
    public class MovingLayer : BaseLayer
    {
        
        public override void OnEnter(Movement moveStatsManager)
        {
            moveStatsManager.transform.SetParent(transform);
        }

        public override void OnExit(Movement moveStatsManager)
        {
            moveStatsManager.transform.SetParent(null);
        }

        public override void OnFixedUpdate(Movement movement)
        {
        }
    }
}