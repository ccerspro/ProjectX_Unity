using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//8/15/2025: Add this script to flashlight to add lag effect when moving camera
public class FlashlightLag : MonoBehaviour
{
    [Header("Follow target (usually the main camera)")]
    public Transform target;

    [Header("Offsets relative to target")]
    public Vector3 localOffset = new Vector3(0f, -0.05f, 0.15f); // tweak to taste

    [Header("Lag / drag (seconds to halve the error)")]
    [Tooltip("Lower = snappier, Higher = heavier. 0.06–0.15 feels good.")]
    public float rotationHalfLife = 0.09f;
    public float positionHalfLife = 0.06f;

    [Header("Limits / polish")]
    public bool lockRoll = true;           // keep flashlight upright
    public float maxOffsetAngle = 15f;     // cap how far it can trail behind (degrees). 0 = unlimited

    void Awake()
    {
        if (!target && Camera.main) target = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (!target) return;

        // Desired pose from camera
        Vector3 desiredPos = target.TransformPoint(localOffset);
        Quaternion desiredRot = target.rotation;
        if (lockRoll) desiredRot = Quaternion.LookRotation(target.forward, Vector3.up);

        // Exponential smoothing (half-life based, framerate independent)
        float pd = DampFactor(positionHalfLife, Time.deltaTime);
        float rd = DampFactor(rotationHalfLife, Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, desiredPos, pd);
        Quaternion smoothed = Quaternion.Slerp(transform.rotation, desiredRot, rd);

        // Optional: clamp how far behind we can be
        if (maxOffsetAngle > 0f)
        {
            float ang = Quaternion.Angle(desiredRot, smoothed);
            if (ang > maxOffsetAngle)
                smoothed = Quaternion.RotateTowards(desiredRot, smoothed, maxOffsetAngle);
        }

        transform.rotation = smoothed;
    }

    // Converts half-life to a lerp factor for this frame
    static float DampFactor(float halfLife, float dt)
    {
        if (halfLife <= 0f) return 1f; // snap
        // 1 - 2^(-dt/halfLife)
        return 1f - Mathf.Pow(2f, -dt / halfLife);
    }
}
