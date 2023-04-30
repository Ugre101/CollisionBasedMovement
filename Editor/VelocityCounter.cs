using TMPro;
using UnityEngine;

public class VelocityCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI counter;
    [SerializeField] Rigidbody player;

    void Update()
    {
        if (Time.frameCount % 3 != 0) return;
        counter.text = $"Velocity: {player.velocity.magnitude:#0.#}\tFall Velocity: {-player.velocity.y:#0.#}";
    }
}