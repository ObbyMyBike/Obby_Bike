using UnityEngine;

public class PingPongObstacle : BaseObstacle
{
    [SerializeField] private Vector3 _offset = Vector3.up;
    [SerializeField] private float _speed = 1f;

    private Vector3 _startPosition;

    private void Start()
    {
        _startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        float time = Mathf.PingPong(Time.time * _speed, 1f);
        
        transform.position = _startPosition + _offset * time;
    }
}