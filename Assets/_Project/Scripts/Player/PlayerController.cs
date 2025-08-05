using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private const string STOP_JUMP = "StopJumpNoPos";
    private const string START_JUMP = "JumpStartNoPos";
    private const string IN_AIR = "InAirNoPos";
    private const string ANIMATION = "anim";
    
    public event Action<float> SpeedBoostStarted;
    public event Action SpeedBoostEnded;
    public event Action<float> JumpBoostStarted;
    public event Action JumpBoostEnded;
    
    [SerializeField] private PlayerConfig _playerConfig;
    [SerializeField] private Animator _animator;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Transform _visualRootTransform;
    
    private readonly float _landingWindowDuration = 0.2f;
    
    private JumpSystem _jumpSystemHandler;
    private BotPusher _botPusher;
    private Animations _animationHandler;
    private BikeMovement _bikeMovement;
    private BoostManager _boostManager;

    private IInput _input;
    private Rigidbody _rigidbody;
    private LayerMask _layerMask;

    private Coroutine _jumpAnimationCoroutine;
    private Coroutine _instantSpeedBoostRoutine;
    
    private float _justLandedWindowTimer;
    
    private bool _jumpPressedThisFrame;
    private bool _wasGroundedLastFrame;
    private bool _forcedAirborne;
    
    public bool LandingEligible => _justLandedWindowTimer > 0f && _rigidbody.velocity.y <= 0f;
    
    private float BaseSpeed => _playerConfig != null ? _playerConfig.MaxSpeed : 0f;
    private bool IsGrounded => _bikeMovement != null && _bikeMovement.IsGrounded;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        _layerMask = ~((1 << gameObject.layer) | (1 << 2));
        
        _jumpSystemHandler = new JumpSystem(_playerConfig.JumpForce, _audioSource, _playerConfig.JumpClip);
        _bikeMovement = new BikeMovement(_rigidbody, transform, _playerConfig, _layerMask, _jumpSystemHandler, _playerConfig.JumpDistance);
        _animationHandler = new Animations(_animator);
        _botPusher = new BotPusher(transform, _playerConfig.PushRadius, _playerConfig.PushForce, _playerConfig.PushCooldown, _playerConfig.PushDuration);
        _boostManager = new BoostManager(_playerConfig, _jumpSystemHandler, this);
        
        _boostManager.SpeedBoostStarted += f => SpeedBoostStarted?.Invoke(f);
        _boostManager.SpeedBoostEnded += () => SpeedBoostEnded?.Invoke();
        _boostManager.JumpBoostStarted += f => JumpBoostStarted?.Invoke(f);
        _boostManager.JumpBoostEnded += () => JumpBoostEnded?.Invoke();
    }

    private void Update()
    {
        UpdateController();
    }
    
    private void FixedUpdate()
    {
        bool wasGroundedBefore = IsGrounded;

        UpdatePhysics(wasGroundedBefore);
        
        _wasGroundedLastFrame = _bikeMovement.IsGrounded;
        _jumpPressedThisFrame = false;
    }

    private void OnDisable()
    {
        _boostManager.Reset();
    }

    public void SetAnimator(Animator animator)
    {
        _animator = animator;
        _animator.Rebind();
    }
    
    public void SetInput(IInput input)
    {
        if (_input != null)
        {
            _input.Jumped -= OnJumped;
            _input.Pushed -= OnManualPush;
        }

        _input = input;

        if (_input != null)
        {
            _input.Jumped += OnJumped;
            _input.Pushed += OnManualPush;
        }
    }

    public void MarkAirborne() => _forcedAirborne = true;
    
    public void ApplyTemporaryJumpBoost(float newJumpForce, float duration) => _boostManager.ApplyJumpBoost(newJumpForce, duration);
    
    public void ApplyTemporarySpeedBoost(float newSpeed, float duration) => _boostManager.ApplySpeedBoost(newSpeed, duration);
    
    public void ApplyInstantSpeedBoost(float speedMultiplier, float decelerationTime)
    {
        if (_instantSpeedBoostRoutine != null)
            StopCoroutine(_instantSpeedBoostRoutine);

        _instantSpeedBoostRoutine = StartCoroutine(InstantSpeedBoostRoutine(speedMultiplier, decelerationTime));
    }

    private void UpdatePhysics(bool wasGroundedBefore)
    {
        bool isGroundedBefore = wasGroundedBefore;
        bool isCurrentlyGrounded = _bikeMovement.IsGrounded;

        if (_jumpPressedThisFrame && isCurrentlyGrounded)
        {
            _animationHandler.OnJumpTriggered();

            if (_jumpAnimationCoroutine != null)
                StopCoroutine(_jumpAnimationCoroutine);

            _jumpAnimationCoroutine = StartCoroutine(JumpAnimationSequence());
            _bikeMovement.TryJump(true);
        }

        _bikeMovement.UpdatePhysics();
        _animationHandler.UpdateAnimator(_rigidbody);

        bool nowGrounded = _bikeMovement.IsGrounded;
        bool justLanded = ((isGroundedBefore == false) || _forcedAirborne) && nowGrounded && _rigidbody.velocity.y <= 0f;

        if (justLanded)
        {
            _justLandedWindowTimer = _landingWindowDuration;
            _forcedAirborne = false;
        }
        else
        {
            _justLandedWindowTimer = Mathf.Max(0f, _justLandedWindowTimer - Time.fixedDeltaTime);
        }

        if (nowGrounded && !_wasGroundedLastFrame && _jumpAnimationCoroutine != null)
        {
            StopCoroutine(_jumpAnimationCoroutine);
            _animator.Play(STOP_JUMP);
            _jumpAnimationCoroutine = StartCoroutine(PlayGroundedAnimation());
        }
    }
    
    private void UpdateController()
    {
        if (_input != null)
            _bikeMovement.UpdateInput(_input);
        
        _botPusher.Tick();
    }

    private void OnJumped() => _jumpPressedThisFrame = true;

    private void OnManualPush()
    {
        bool pushed = _botPusher.TryPush();
        
        if (pushed)
        {
            if (_playerConfig != null && _playerConfig.PushClip != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_playerConfig.PushClip);
            }
        }
    }
    
    private IEnumerator JumpAnimationSequence()
    {
        _animator.Play(START_JUMP);
        
        yield return null;
        
        while (_jumpSystemHandler.IsGrounded(transform, _playerConfig.JumpDistance, _layerMask))
            yield return null;

        _animator.CrossFade(IN_AIR, 0.2f);
        
        while (!_jumpSystemHandler.IsGrounded(transform, _playerConfig.JumpDistance, _layerMask))
            yield return null;
    }

    private IEnumerator PlayGroundedAnimation()
    {
        var state = _animator.GetCurrentAnimatorStateInfo(0);
        
        while (state.IsName(STOP_JUMP) && state.normalizedTime < 1f)
        {
            yield return null;
            state = _animator.GetCurrentAnimatorStateInfo(0);
        }

        _animator.CrossFade(ANIMATION, 0.2f);
        _jumpAnimationCoroutine = null;
    }
    
    private IEnumerator InstantSpeedBoostRoutine(float speedMultiplier, float decelerationTime)
    { 
        Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        Vector3 forwardDirection = transform.forward;
        
        forwardDirection.y = 0f;
        forwardDirection.Normalize();

        float currentForwardSpeed = Vector3.Dot(flatVelocity, forwardDirection);
        
        if (currentForwardSpeed < 0f)
            currentForwardSpeed = 0f;

        float baseSpeed = BaseSpeed;
        float targetSpeed = baseSpeed * speedMultiplier;
        float deltaSpeed = targetSpeed - currentForwardSpeed;
        
        _rigidbody.velocity += forwardDirection * deltaSpeed;

        float timer = decelerationTime;
        
        while (timer > 0f)
        {
            float time = 1f - (timer / decelerationTime);
            float desiredForward = Mathf.Lerp(targetSpeed, baseSpeed, time);

            Vector3 currentFlat = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
            float currentForward = Vector3.Dot(currentFlat, forwardDirection);
            float adjust = desiredForward - currentForward;

            if (Mathf.Abs(adjust) > 0.001f)
                _rigidbody.velocity += forwardDirection * adjust;

            timer -= Time.deltaTime;
            
            yield return null;
        }

        _instantSpeedBoostRoutine = null;
    }
}