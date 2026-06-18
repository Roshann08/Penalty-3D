using UnityEngine;

/// <summary>
/// Mouse/keyboard aiming and kicking controller.
/// Attach to any persistent GameObject (e.g. GameManager or Camera).
///
/// Controls:
///   - Move mouse left/right to aim horizontally
///   - Hold Left Mouse Button and drag up to charge power
///   - Release LMB to kick
///   - A/D keys for curl (side spin)
/// </summary>
public class BallLauncher : MonoBehaviour
{
    [Header("References")]
    public GameManager    gameManager;
    public BallController ball;
    public Transform      goalTransform;     // Goal centre — used to clamp aim direction

    [Header("Aim")]
    [Range(10f, 60f)]
    public float maxHorizontalAngle = 35f;   // degrees left/right from straight
    [Range(0f, 45f)]
    public float defaultLiftAngle   = 15f;   // default upward angle

    [Header("Power")]
    public float minPower = 0.4f;
    public float maxPower = 1.0f;
    [Tooltip("Pixels of upward mouse drag for full power")]
    public float dragPixelsForMaxPower = 200f;

    [Header("Curl")]
    public float maxCurl = 1f;

    [Header("Aim Indicator")]
    public LineRenderer aimLine;             // optional — drag a LineRenderer here

    bool    _isCharging;
    float   _chargeStartY;
    float   _currentPower;
    float   _aimAngleH;   // degrees, relative to forward
    float   _curlAmount;

    void Update()
    {
        if (gameManager.CurrentState != GameManager.State.Aiming) return;

        HandleAim();
        HandlePowerCharge();
        HandleCurl();
        UpdateAimLine();
    }

    void HandleAim()
    {
        // Map mouse X to horizontal aim angle
        float screenFraction = (Input.mousePosition.x / Screen.width) * 2f - 1f; // -1 to +1
        _aimAngleH = screenFraction * maxHorizontalAngle;
    }

    void HandlePowerCharge()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isCharging    = true;
            _chargeStartY  = Input.mousePosition.y;
        }

        if (_isCharging && Input.GetMouseButton(0))
        {
            float dragUp  = Input.mousePosition.y - _chargeStartY;
            float t       = Mathf.Clamp01(dragUp / dragPixelsForMaxPower);
            _currentPower = Mathf.Lerp(minPower, maxPower, t);
        }

        if (_isCharging && Input.GetMouseButtonUp(0))
        {
            _isCharging = false;
            FireKick();
        }
    }

    void HandleCurl()
    {
        float curl = 0f;
        if (Input.GetKey(KeyCode.A)) curl = -maxCurl;
        if (Input.GetKey(KeyCode.D)) curl =  maxCurl;
        _curlAmount = curl;
    }

    void FireKick()
    {
        Vector3 direction = BuildKickDirection();
        gameManager.OnKick(direction, _currentPower, _curlAmount);
        _currentPower = 0f;
    }

    Vector3 BuildKickDirection()
    {
        // Rotate forward vector by horizontal aim angle and lift
        Quaternion hRot   = Quaternion.AngleAxis(_aimAngleH, Vector3.up);
        Quaternion vRot   = Quaternion.AngleAxis(defaultLiftAngle, Vector3.right);
        Vector3 direction = hRot * vRot * Vector3.forward;
        return direction.normalized;
    }

    void UpdateAimLine()
    {
        if (aimLine == null) return;

        Vector3 dir   = BuildKickDirection();
        Vector3 start = ball.transform.position;
        aimLine.SetPosition(0, start);
        aimLine.SetPosition(1, start + dir * 3f);
    }

    // Called externally (e.g. Azure Kinect) to kick programmatically
    public void KickFromExternal(Vector3 direction, float power, float sideOffset = 0f)
    {
        gameManager.OnKick(direction, power, sideOffset);
    }
}
