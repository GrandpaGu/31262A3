using UnityEngine;

/// <summary>
/// Follows a target with SmoothDamp + dead-zone + horizontal look-ahead.
/// Attach to the Main Camera.
/// </summary>
public class CameraFollowYOnly : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                 // Player (or any object) to track
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Dead-zone (in world units)")]
    public float deadZoneWidth = 2f;        // How far the player can move horizontally before the camera pans
    public float deadZoneHeight = 1.5f;      // Vertical slack

    [Header("Look-ahead")]
    public float lookAheadDistance = 2f;   // Max pixels/units the camera peeks forward
    public float lookAheadSmoothTime = 0.1f; // How quickly the peek catches up / eases back

    [Header("Follow smoothing")]
    public float smoothTime = 0.15f;         // Damping for the main camera movement

    // ©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤
    Vector3 _currentVelocity = Vector3.zero; // For SmoothDamp
    float _currentLookAhead = 0f;          // Smoothed look-ahead offset
    float _lookAheadVel = 0f;          // For look-ahead SmoothDamp
    Vector3 _prevTargetPos;

    void Start()
    {
        if (target) _prevTargetPos = target.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        /*©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤*
         *   DEAD-ZONE LOGIC    *
         *©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤*/
        // Distance between player and camera centre (ignoring offset & Z)
        Vector2 delta = target.position - (transform.position - offset);

        Vector3 desired = transform.position;            // Start from current pos

        if (Mathf.Abs(delta.x) > deadZoneWidth * 0.5f)   // Outside X dead-zone?
            desired.x = target.position.x
                        - Mathf.Sign(delta.x) * deadZoneWidth * 0.5f
                        + offset.x;

        if (Mathf.Abs(delta.y) > deadZoneHeight * 0.5f)  // Outside Y dead-zone?
            desired.y = target.position.y
                        - Mathf.Sign(delta.y) * deadZoneHeight * 0.5f
                        + offset.y;

        /*©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤*
         *   LOOK-AHEAD LOGIC  *
         *©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤*/
        Vector3 targetMovement = target.position - _prevTargetPos;
        float moveDir = Mathf.Sign(targetMovement.x);
        float lookAheadTarget = (Mathf.Abs(targetMovement.x / Time.deltaTime) > 0.01f)
                                 ? moveDir * lookAheadDistance
                                 : 0f;

        _currentLookAhead = Mathf.SmoothDamp(_currentLookAhead,
                                             lookAheadTarget,
                                             ref _lookAheadVel,
                                             lookAheadSmoothTime);

        desired.x += _currentLookAhead;      // Apply peek

        /*©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤*
         *   FINAL SMOOTH MOVE *
         *©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤©¤*/
        transform.position = Vector3.SmoothDamp(transform.position,
                                                desired,
                                                ref _currentVelocity,
                                                smoothTime);

        _prevTargetPos = target.position;
    }
}