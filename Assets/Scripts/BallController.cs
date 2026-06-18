using UnityEngine;

/// <summary>
/// Controls the ball physics and state.
/// Attach to the football GameObject alongside a Rigidbody and SphereCollider.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class BallController : MonoBehaviour
{
    [Header("Physics")]
    public float mass           = 0.43f;   // regulation ball kg
    public float drag           = 0.1f;
    public float angularDrag    = 0.05f;

    [Header("Kick")]
    [Range(5f, 40f)]
    public float maxKickForce   = 25f;

    [Header("Spin")]
    public float spinMultiplier = 0.5f;    // torque applied relative to kick direction offset

    public bool IsInFlight { get; private set; }
    public Vector3 StartPosition { get; private set; }

    Rigidbody _rb;
    Vector3   _spawnPos;
    Quaternion _spawnRot;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.mass        = mass;
        _rb.linearDamping    = drag;
        _rb.angularDamping = angularDrag;
        _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        _spawnPos = transform.position;
        _spawnRot = transform.rotation;
        StartPosition = _spawnPos;
    }

    /// <summary>
    /// Kicks the ball.
    /// </summary>
    /// <param name="direction">Normalised world-space direction.</param>
    /// <param name="forceMagnitude">0–1 power factor (will be scaled to maxKickForce).</param>
    /// <param name="sideOffset">-1 (left curl) to +1 (right curl) spin bias.</param>
    public void Kick(Vector3 direction, float forceMagnitude, float sideOffset = 0f)
    {
        if (IsInFlight) return;

        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        float force = forceMagnitude * maxKickForce;
        _rb.AddForce(direction * force, ForceMode.Impulse);

        // Apply spin torque for curl effect
        Vector3 spinAxis = Vector3.Cross(direction, Vector3.up);
        _rb.AddTorque(spinAxis * sideOffset * force * spinMultiplier, ForceMode.Impulse);

        IsInFlight = true;
    }

    /// <summary>
    /// Resets ball to spawn position and freezes it.
    /// </summary>
    public void ResetBall()
    {
        _rb.isKinematic = true;
        transform.position = _spawnPos;
        transform.rotation = _spawnRot;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        IsInFlight = false;
    }

    void OnCollisionEnter(Collision col)
    {
        // Ball has landed / hit something — no longer in clean flight
        if (IsInFlight && col.gameObject.CompareTag("Ground"))
        {
            IsInFlight = false;
        }
    }
}
