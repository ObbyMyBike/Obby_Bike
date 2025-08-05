using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BotRespawner))]
public class BotController : MonoBehaviour
{
    [SerializeField] private Animator _animator;
 
    [Header("Movement Settings")]
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _acceleration = 100f;
    [SerializeField] private float _angularSpeed = 720f;
    
    [Header("Jump Settings")]
    [SerializeField] private float _jumpForce = 30f;
    [SerializeField] private float _jumpDistance = 1.2f;
    
    [Header("Push & Respawn")]
    [SerializeField] private float _respawnDelay = 0.5f;

    private IInput _input;
    private Movement _movementHandler;
    private JumpSystem _jumpSystem;
    private Animations _animationHandler;
    private BotRespawner _respawner;
    
    private Rigidbody _rigidbody;
    private Coroutine _pushCoroutine;
    
    private Vector3 _pendingPush;
    private Vector3 _moveDirectionWorld;
    private Vector3 _lastVelocityDirection;
    private LayerMask _layerMask;
    
    private bool _jumpPressedThisFrame;
    private bool _wasGroundedLastFrame;
    private bool _isPushed;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _respawner = GetComponent<BotRespawner>();
        
        _rigidbody.freezeRotation = true;

        _layerMask = ~((1 << gameObject.layer) | (1 << 2));

        _movementHandler = new Movement();
        _jumpSystem = new JumpSystem(_jumpForce);
        _animationHandler = new Animations(_animator);
    }

    private void Update()
    {
        UpdateController();
    }

    private void FixedUpdate()
    {
        if (_pendingPush.sqrMagnitude > 0.0001f)
        {
            _rigidbody.velocity = _pendingPush;
            _pendingPush = Vector3.zero;
            
            return;
        }
        
        if (_isPushed)
            return;
        
        bool isGrounded = _jumpSystem.IsGrounded(transform, _jumpDistance, _layerMask);
        
        if (_moveDirectionWorld.sqrMagnitude > 0.0001f)
        {
            _movementHandler.HandleMovement(_rigidbody, _moveDirectionWorld, isGrounded, _speed, _acceleration);
            _movementHandler.ClampSpeed(_rigidbody, _speed);
        }

        _animationHandler.UpdateAnimator(_rigidbody);
    }

    public void SetInput(IInput input)
    {
        if (_input != null)
            _input.Jumped -= OnJumped;

        _input = input;

        if (_input != null)
            _input.Jumped += OnJumped;
    }
    
    public void ApplyPush(Vector3 worldVelocity, float pushDuration)
    {
        _pendingPush = worldVelocity;
        _isPushed = true;
        
        if (_pushCoroutine != null)
            StopCoroutine(_pushCoroutine);
        
        _pushCoroutine = StartCoroutine(EndPushAfter(pushDuration));
    }

    private void UpdateController()
    {
        if (_isPushed)
            return;
        
        bool isGrounded = _jumpSystem.IsGrounded(transform, _jumpDistance, _layerMask);

        if (_jumpPressedThisFrame)
        {
            _jumpSystem.TryJump(_rigidbody, _moveDirectionWorld, isGrounded);
            _animationHandler.OnJumpTriggered();
        }
        
        UpdateMoveDirection();
        RotateTowardsVelocity();

        _animationHandler.HandleJumpAnimation(_wasGroundedLastFrame, isGrounded, _jumpPressedThisFrame);

        _wasGroundedLastFrame = isGrounded;
        _jumpPressedThisFrame = false;
    }

    private void UpdateMoveDirection()
    {
        if (_input != null)
        {
            Vector2 raw = _input.InputDirection;
            _moveDirectionWorld = new Vector3(raw.x, 0f, raw.y).normalized;
        }
        else
        {
            _moveDirectionWorld = Vector3.zero;
        }
    }

    private void RotateTowardsVelocity()
    {
        if (_rigidbody.velocity.sqrMagnitude > 0.16f)
        {
            _lastVelocityDirection = _rigidbody.velocity;
            _lastVelocityDirection.y = 0f;

            if (_lastVelocityDirection.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(_lastVelocityDirection.normalized);
                transform.rotation =
                    Quaternion.RotateTowards(transform.rotation, targetRot, _angularSpeed * Time.deltaTime);
            }
        }
    }

    private void OnJumped() => _jumpPressedThisFrame = true;
    
    private IEnumerator EndPushAfter(float duration)
    {
        yield return new WaitForSeconds(duration);
        
        _isPushed = false;
        _pushCoroutine = null;
    }
}