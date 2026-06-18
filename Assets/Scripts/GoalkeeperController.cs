using System.Collections;
using UnityEngine;

/// <summary>
/// Controls goalkeeper movement and animation.
/// Attach to the Saha.fbx GameObject. Requires an Animator with the states listed below.
///
/// Required Animator parameters:
///   - DiveLeft  (Trigger)
///   - DiveRight (Trigger)
///   - DiveUp    (Trigger)
///   - Idle      (Trigger)  — used to return to idle after a dive
/// </summary>
[RequireComponent(typeof(Animator))]
public class GoalkeeperController : MonoBehaviour
{
    [Header("Dive Settings")]
    [Tooltip("How quickly the keeper slides to dive position (m/s)")]
    public float diveSpeed      = 8f;
    [Tooltip("Max horizontal distance the keeper can dive from centre")]
    public float maxDiveX       = 2.8f;
    [Tooltip("Max vertical height on a high dive")]
    public float maxDiveY       = 1.6f;
    [Tooltip("Seconds before keeper reacts after ball is kicked")]
    public float reactionDelay  = 0.15f;
    [Tooltip("Seconds after a save/miss before keeper resets")]
    public float resetDelay     = 2.0f;

    [Header("Idle Sway")]
    public float swayAmount     = 0.3f;
    public float swaySpeed      = 1.2f;

    Animator   _anim;
    Vector3    _originPos;
    bool       _hasDived;
    Coroutine  _diveRoutine;

    // Animator parameter hashes for performance
    static readonly int HashDiveLeft  = Animator.StringToHash("DiveLeft");
    static readonly int HashDiveRight = Animator.StringToHash("DiveRight");
    static readonly int HashDiveUp    = Animator.StringToHash("DiveUp");
    static readonly int HashIdle      = Animator.StringToHash("Idle");

    void Awake()
    {
        _anim      = GetComponent<Animator>();
        _originPos = transform.position;
    }

    void Update()
    {
        if (!_hasDived)
            ApplyIdleSway();
    }

    void ApplyIdleSway()
    {
        float x = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        transform.position = _originPos + new Vector3(x, 0f, 0f);
    }

    /// <summary>
    /// Called by GameManager when the ball is kicked.
    /// targetPos is the predicted ball position at the goal line.
    /// </summary>
    public void ReactToBall(Vector3 targetPos)
    {
        if (_hasDived) return;
        if (_diveRoutine != null) StopCoroutine(_diveRoutine);
        _diveRoutine = StartCoroutine(DiveRoutine(targetPos));
    }

    IEnumerator DiveRoutine(Vector3 targetPos)
    {
        yield return new WaitForSeconds(reactionDelay);

        _hasDived = true;

        // Determine dive type from target position relative to keeper origin
        Vector3 localTarget = targetPos - _originPos;
        float   absX        = Mathf.Abs(localTarget.x);
        bool    isHigh      = targetPos.y > 1.2f;

        // Clamp dive destination
        float diveX = Mathf.Clamp(localTarget.x, -maxDiveX, maxDiveX);
        float diveY = isHigh ? maxDiveY : 0f;
        Vector3 diveTarget = _originPos + new Vector3(diveX, diveY, 0f);

        // Trigger correct animation
        if (isHigh)
            _anim.SetTrigger(HashDiveUp);
        else if (localTarget.x < -0.5f)
            _anim.SetTrigger(HashDiveLeft);
        else if (localTarget.x > 0.5f)
            _anim.SetTrigger(HashDiveRight);
        // else stay centre — no animation trigger needed

        // Slide keeper toward dive target
        float elapsed = 0f;
        float duration = Vector3.Distance(transform.position, diveTarget) / diveSpeed;
        duration = Mathf.Max(duration, 0.01f);
        Vector3 startPos = transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, diveTarget, elapsed / duration);
            yield return null;
        }
        transform.position = diveTarget;

        // Wait, then reset
        yield return new WaitForSeconds(resetDelay);
        ResetKeeper();
    }

    public void ResetKeeper()
    {
        _hasDived = false;
        transform.position = _originPos;
        _anim.SetTrigger(HashIdle);
    }
}
