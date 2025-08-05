using System.Collections;
using UnityEngine;

public class PlayerTarget : MonoBehaviour
{
    private Coroutine _decelerationRoutine;
    
    public Rigidbody Rigidbody { get; private set; }
    public bool IsBoosting { get; private set; }

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    public void Boost(Vector3 direction, float speedMultiplier, float decelerationTime)
    {
        IsBoosting = true;
        
        if (_decelerationRoutine != null)
            StopCoroutine(_decelerationRoutine);
        
        float currentSpeed = Rigidbody.velocity.magnitude;
        float baseSpeed = Mathf.Max(currentSpeed, 1f);

        Vector3 dir = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        dir.y = 0f;
        dir.Normalize();
        
        Vector3 newVelocity = dir * baseSpeed * speedMultiplier;
        newVelocity.y = 0f;

        Rigidbody.velocity = newVelocity;

        if (decelerationTime > 0f)
            _decelerationRoutine = StartCoroutine(DecelerateTo(currentSpeed, decelerationTime));
        else
            IsBoosting = false;
    }

    public void BoostArc(Vector3 velocity)
    { 
        Rigidbody.velocity = velocity;
    }
    
    public void StopBoost()
    {
        if (_decelerationRoutine != null)
        {
            StopCoroutine(_decelerationRoutine);
            
            _decelerationRoutine = null;
        }

        IsBoosting = false;
    }

    private IEnumerator DecelerateTo(float finalSpeed, float time)
    {
        float startTime = Time.time;
        Vector3 startVelocity = Rigidbody.velocity;
        float startSpeed = new Vector2(startVelocity.x, startVelocity.z).magnitude;

        while (Time.time - startTime < time)
        {
            float t = (Time.time - startTime) / time;
            float speed = Mathf.Lerp(startSpeed, finalSpeed, t);
            
            Vector3 horizontalDirection = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z).normalized;
            Rigidbody.velocity = horizontalDirection * speed;

            yield return null;
        }
        
        
        if (Rigidbody.velocity.sqrMagnitude > 0.0001f)
        {
            Vector3 horDir = new Vector3(Rigidbody.velocity.x, 0f, Rigidbody.velocity.z).normalized;
            Rigidbody.velocity = horDir * finalSpeed;
        }
        
        IsBoosting = false;
    }
}