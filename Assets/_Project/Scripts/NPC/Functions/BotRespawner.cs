using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BotRespawner : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _respawnCooldown = 0.5f;
    [SerializeField, Min(0f)] private float _respawnRetryInterval = 0.2f;
    
    private BotInput _botInput;
    private Waypoint _startPoint;
    private Waypoint _lastCheckpointWaypoint;
    private CheckPoints _lastCheckpoint;
    
    private Rigidbody _rigidbody;
    private Vector3 _lastCheckpointPosition;
    
    private float _lastRespawnTime = -Mathf.Infinity;
    private bool _waitingForClear;
    private bool _hasCheckpoint;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>() ?? GetComponentInParent<Rigidbody>();
    }

    public void Initialize(BotInput botInput, Waypoint startPoint)
    {
        _botInput = botInput;
        _startPoint = startPoint;
        _lastCheckpointWaypoint = startPoint;
        _lastCheckpointPosition = startPoint.transform.position;
        _hasCheckpoint = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CheckPoints checkpoint))
            SetCheckpoint(checkpoint);
    }

    public void SetCheckpoint(CheckPoints checkpoint)
    {
        if (checkpoint == null)
            return;

        _lastCheckpoint = checkpoint;
        _lastCheckpointPosition = checkpoint.transform.position;
        _hasCheckpoint = true;
        
        if (checkpoint.AssociatedWaypoint != null)
            _lastCheckpointWaypoint = checkpoint.AssociatedWaypoint;
    }
    
    public void Respawn()
    {
        if (_botInput == null)
            return;

        if (Time.time - _lastRespawnTime < _respawnCooldown)
            return;

        if (!_hasCheckpoint && _startPoint != null)
        {
            _lastCheckpointPosition = _startPoint.transform.position;
            _lastCheckpointWaypoint = _startPoint;
        }
        
        if (_lastCheckpoint is CheckPoints checkpoint)
        {
            if (!checkpoint.CanSpawnOrRespawnHere())
            {
                if (!_waitingForClear)
                    StartCoroutine(RetryRespawnUntilClear(checkpoint));
                
                return;
            }
        }

        _lastRespawnTime = Time.time;
        
        DoRespawn();
    }
    
    private void DoRespawn()
    {
        transform.position = _lastCheckpointPosition;

        if (_lastCheckpointWaypoint != null)
        {
            Vector3 flat = _lastCheckpointWaypoint.transform.position - transform.position;
            flat.y = 0;

            if (flat.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(flat.normalized);
        }

        if (_rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        _botInput.ResetToWaypoint(_lastCheckpointWaypoint);
        _botInput.Tick();
    }

    private IEnumerator RetryRespawnUntilClear(CheckPoints checkpoint)
    {
        _waitingForClear = true;

        while (true)
        {
            yield return new WaitForSeconds(_respawnRetryInterval);
            
            if (checkpoint.CanSpawnOrRespawnHere())
                break;
        }

        _waitingForClear = false;
        _lastRespawnTime = Time.time;
        
        DoRespawn();
    }
}