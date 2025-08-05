using UnityEngine;

public class BotPusher
{
    private readonly Transform _playerTransform;
    private readonly float _pushRadius;
    private readonly float _pushForce;
    private readonly float _cooldown;
    private readonly float _pushDuration;

    private float _lastPushTime = -Mathf.Infinity;

    public BotPusher(Transform playerTransform, float pushRadius, float pushForce, float cooldown, float pushDuration)
    {
        _playerTransform = playerTransform;
        _pushRadius = pushRadius;
        _pushForce = pushForce;
        _cooldown = cooldown;
        _pushDuration = pushDuration;
    }

    public void Tick()
    {
        TryPush();
    }
    
    public bool TryPush()
    {
        float now = Time.time;
        
        if (now < _lastPushTime + _cooldown)
            return false;

        if (!FindAndPushNearestBot())
            return false;

        _lastPushTime = now;
        
        return true;
    }

    private bool FindAndPushNearestBot()
    {
        Vector3 playerPosition = _playerTransform.position;
        Collider[] hits = Physics.OverlapSphere(playerPosition, _pushRadius, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

        Collider nearest = null;
        BotController botController = null;
        float bestSqr = float.MaxValue;

        foreach (Collider collider in hits)
        {
            if (!collider.TryGetComponent(out BotController bot))
                continue;

            float distance = (collider.transform.position - playerPosition).sqrMagnitude;

            if (distance < bestSqr)
            {
                bestSqr = distance;
                nearest = collider;
                botController = bot;
            }
        }

        if (botController == null)
            return false;

        Vector3 direction = nearest.transform.position - playerPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
            direction.Normalize();
        else
            direction = Vector3.zero;

        botController.ApplyPush(direction * _pushForce, _pushDuration);
        
        return true;
    }
}