using UnityEngine;

public class CatapultLaunch
{
    public void TryLaunch(PlayerTarget target, BoostZonePreset preset, Transform launchPoint)
    {
        if (launchPoint == null || preset == null || target == null)
            return;

        Rigidbody rigidbody = target.Rigidbody;
        
        if (rigidbody == null)
            return;
        
        rigidbody.position = launchPoint.position;
        
        Vector3 heightRight = new Vector3(launchPoint.right.x, 0f, launchPoint.right.z);
        Vector3 heightForward = new Vector3(launchPoint.forward.x, 0f, launchPoint.forward.z);

        if (heightRight.sqrMagnitude < 0.0001f || heightForward.sqrMagnitude < 0.0001f)
            return;

        heightRight.Normalize();
        heightForward.Normalize();

        Vector3 offset = heightRight * preset.LaunchDistanceX + heightForward * preset.LaunchDistanceZ;
        float horizontalDistance = new Vector3(offset.x, 0f, offset.z).magnitude;
        
        if (horizontalDistance < 0.001f)
            return;

        Vector3 horizontalDir = offset.normalized;

        float angleRad = preset.LaunchAngle * Mathf.Deg2Rad;
        float gravity = Mathf.Abs(Physics.gravity.y);
        float deltaHeight = preset.LandingHeight;
        float cosA = Mathf.Cos(angleRad);
        float sinA = Mathf.Sin(angleRad);
        float tanA = Mathf.Tan(angleRad);
        float denom = 2f * cosA * cosA * (horizontalDistance * tanA - deltaHeight);
        
        if (denom <= 0f)
            return;

        float speed = Mathf.Sqrt(gravity * horizontalDistance * horizontalDistance / denom);

        Vector3 launchVelocity = horizontalDir * (speed * cosA) + Vector3.up * (speed * sinA);

        rigidbody.velocity = launchVelocity;

        //float flightTime = 2f * speed * sinA / gravity;

        target.BoostArc(launchVelocity);
    }
}