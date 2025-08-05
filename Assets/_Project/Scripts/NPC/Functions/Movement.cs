using UnityEngine;

public class Movement
{
    public void HandleMovement(Rigidbody rigidbody, Vector3 desiredDirection, bool isGrounded, float speed, float acceleration)
    {
        rigidbody.drag = isGrounded ? 5f : 1f;

        if (desiredDirection.sqrMagnitude > 0.0001f)
            rigidbody.AddForce(desiredDirection * acceleration * rigidbody.mass * speed);
    }

    public void ClampSpeed(Rigidbody rigidbody, float maxSpeed)
    {
        Vector3 horizontal = new Vector3(rigidbody.velocity.x, 0f, rigidbody.velocity.z);
        float magnitude = horizontal.magnitude;
        
        if (magnitude > maxSpeed)
        {
            horizontal = horizontal.normalized * maxSpeed;
            rigidbody.velocity = new Vector3(horizontal.x, rigidbody.velocity.y, horizontal.z);
        }
    }
}