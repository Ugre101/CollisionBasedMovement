using UnityEngine;

namespace AvatarScripts
{
    [RequireComponent(typeof(CapsuleCollider))]
    public class CharacterCapsule : MonoBehaviour
    {
        [SerializeField] CapsuleCollider capsule;

        [SerializeField, HideInInspector,] Vector3 p1PreCalcMath;
        [SerializeField, HideInInspector,] Vector3 p2PreCalcMath;

        float currentHeight;

        public CapsuleCollider Capsule => capsule;

        public Vector3 P1PreCalcMath => p1PreCalcMath;

        public Vector3 P2PreCalcMath => p2PreCalcMath;
        public float Radius => capsule.radius;

        public Vector3 BottomCenter
        {
            get
            {
                var center = capsule.bounds.center;
                center.y = capsule.bounds.min.y;
                return center;
            }
        }

        public Vector3 Center => Capsule.bounds.center;

        public float Height => capsule.height;
        public float YMax => Capsule.bounds.max.y;
        public float YMin => Capsule.bounds.min.y;

        void Start()
        {
            currentHeight = capsule.height;
            CalcCapsulePosition();
        }

        public void CalcCapsulePosition()
        {
            p1PreCalcMath = Vector3.up * (capsule.radius + Physics.defaultContactOffset);
            p2PreCalcMath = Vector3.up * (capsule.height - capsule.radius + Physics.defaultContactOffset);
        }

        public void SetHeight(float height)
        {
            currentHeight = height;
            capsule.height = currentHeight;
            capsule.center = currentHeight * 0.5f * Vector3.up;
            CalcCapsulePosition();
        }


        public void HalfHeight()
        {
            capsule.height /= 2f;
            capsule.center = capsule.height * 0.5f * Vector3.up;
            CalcCapsulePosition();
        }

        public void RestoreHeight()
        {
            capsule.height = currentHeight;
            capsule.center = currentHeight * 0.5f * Vector3.up;
            CalcCapsulePosition();
        }

        public int OverlapCapsuleNonAlloc(Vector3 capsuleBottom, Collider[] results, LayerMask groundLayer)
            => Physics.OverlapCapsuleNonAlloc(capsuleBottom + P1PreCalcMath, capsuleBottom + P2PreCalcMath, Radius, results,
                groundLayer);
    }
}