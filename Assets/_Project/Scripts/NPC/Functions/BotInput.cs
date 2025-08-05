using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BotInput : IInput
{
    public event Action Jumped;
    public event Action Pushed;

    private Transform _botTransform;
    private Waypoint _currentTarget;

    private bool _jumpQueued = false;

    public BotInput(Transform botTransform, Waypoint start)
    {
        _botTransform = botTransform;
        _currentTarget = start;
    }

    public Vector2 InputDirection { get; private set; }

    public void Tick()
    {
        if (_currentTarget == null)
        {
            InputDirection = Vector2.zero;
            return;
        }

        Vector3 position = _botTransform.position;
        Vector3 target = _currentTarget.transform.position;
        Vector3 toTarget = target - position;
        float distanceSqr = toTarget.sqrMagnitude;
        float activateSqr = _currentTarget.ActivationRadius * _currentTarget.ActivationRadius;

        if (distanceSqr < activateSqr)
        {
            if (_currentTarget.RequiresJump && !_jumpQueued)
            {
                Jumped?.Invoke();
                
                _jumpQueued = true;
            }
            
            List<Waypoint> next = _currentTarget.NextWaypoints;

            if (next != null && next.Count > 0)
                _currentTarget = next[Random.Range(0, next.Count)];

            _jumpQueued = false;
            
            target = _currentTarget.transform.position;
            toTarget = target - position;
        }

        Vector3 direction = toTarget.normalized;
        InputDirection = new Vector2(direction.x, direction.z);
    }

    public void ResetToWaypoint(Waypoint waypoint)
    {
        if (waypoint == null)
            return;

        _currentTarget = waypoint;
        _jumpQueued = false;
    }
}