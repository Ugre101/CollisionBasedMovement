using MovementScripts;
using TMPro;
using UnityEngine;

public class SlopeText : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI texT;

    [SerializeField] GroundCheck groundChecker;

    void Update()
    {
        texT.text = $"Grounded {groundChecker.IsGrounded}\nOn Slope {groundChecker.OnSlope()}\n" +
                    $"Dist Ground {groundChecker.DistanceToGround:0.##}\nGround state {groundChecker.CurrentGroundState}";
        
        //                    $"IsBlocked {groundChecker.IsBlocked}\nCan Climb {groundChecker.CanStepUp}\n" +

    }
}
