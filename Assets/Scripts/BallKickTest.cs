using UnityEngine;

public class BallKickTest : MonoBehaviour
{
    public float force = 10f;

    void Update()
    {
        // Press SPACE to test kick
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Kick(Vector3.forward);
        }
    }

    public void Kick(Vector3 direction)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}