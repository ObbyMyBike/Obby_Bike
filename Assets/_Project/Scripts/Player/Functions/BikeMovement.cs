using UnityEngine;

public class BikeMovement
{
    private readonly Rigidbody _rigidbody;
    private readonly Transform _transform;
    private readonly PlayerConfig _config;
    private readonly JumpSystem _jumpSystem;
    private readonly LayerMask _layerMask;
    private readonly float _jumpDistance;
    private readonly float _climbAssist = 1f;
    private readonly float _maxClimbableSlopeAngle = 50f;
    
    private Vector2 _smoothedInput;
    private Vector2 _rawInput;
    private Vector3 _groundNormal = Vector3.up;
    
    private float _headingYawDegrees;
    private float _targetHeadingYawDegrees;
    private float _headingSmoothVelocity;
    private float _currentLeanAngle;
    private float _leanVelocity;
    
    public bool IsGrounded { get; private set; }

    public BikeMovement(Rigidbody rigidbody, Transform transform, PlayerConfig config, LayerMask layerMask, JumpSystem jumpSystem, float jumpDistance)
    {
        _rigidbody = rigidbody;
        _transform = transform;
        _config = config;
        _layerMask = layerMask;
        _jumpSystem = jumpSystem;
        _jumpDistance = jumpDistance;

        _headingYawDegrees = transform.eulerAngles.y;
        _targetHeadingYawDegrees = _headingYawDegrees;
        _headingSmoothVelocity = 0f;
    }

    public void UpdateInput(IInput input)
    {
        _rawInput = input.InputDirection;
        _smoothedInput = Vector2.Lerp(_smoothedInput, _rawInput, Time.deltaTime * 10f);
    }

    private void UpdateGroundNormal()
    {
        Ray ray = new Ray(_transform.position + Vector3.up * 0.1f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, 1.2f, _layerMask, QueryTriggerInteraction.Ignore))
        {
            _groundNormal = hit.normal;
            IsGrounded = true;
        }
        else
        {
            _groundNormal = Vector3.up;
            IsGrounded = false;
        }
    }

    public void UpdatePhysics()
    {
        float deltaTime = Time.fixedDeltaTime;
        bool jumpSystemGrounded = _jumpSystem.IsGrounded(_transform, _jumpDistance, _layerMask);
        
        UpdateGroundNormal();
        
        IsGrounded = jumpSystemGrounded || IsGrounded;

        Vector3 desiredDirectionWorld = new Vector3(_smoothedInput.x, 0f, _smoothedInput.y);
        
        if (desiredDirectionWorld.sqrMagnitude > 0.0001f)
            desiredDirectionWorld.Normalize();
        else
            desiredDirectionWorld = Vector3.zero;

        bool hasInput = desiredDirectionWorld.sqrMagnitude > 0f;
        
        if (hasInput)
            _targetHeadingYawDegrees = Mathf.Atan2(desiredDirectionWorld.x, desiredDirectionWorld.z) * Mathf.Rad2Deg;

        _headingYawDegrees = Mathf.SmoothDampAngle(_headingYawDegrees, _targetHeadingYawDegrees, ref _headingSmoothVelocity, _config.TurnSmoothTime, float.MaxValue, deltaTime);

        if (hasInput)
        {
            float delta = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
            
            if (Mathf.Abs(delta) < 0.5f)
            {
                _headingYawDegrees = _targetHeadingYawDegrees;
                _headingSmoothVelocity = 0f;
            }
        }

        Quaternion baseHeading = Quaternion.Euler(0f, _headingYawDegrees, 0f);
        Vector3 forwardOnGround = Vector3.ProjectOnPlane(baseHeading * Vector3.forward, _groundNormal).normalized;
        Vector3 rightOnGround = Vector3.ProjectOnPlane(baseHeading * Vector3.right, _groundNormal).normalized;

        float throttle = Mathf.Clamp01(_smoothedInput.magnitude);
        Vector3 velocity = _rigidbody.velocity;
        
        float forwardVelocity = Vector3.Dot(velocity, forwardOnGround);
        float lateralVelocity = Vector3.Dot(velocity, rightOnGround);
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);
        float gravityAlongForward = Vector3.Dot(Physics.gravity, forwardOnGround);
        
        if (throttle > _config.MinInputThreshold)
        {
            float desiredForwardSpeed = throttle * _config.MaxSpeed;
            float speedDifference = desiredForwardSpeed - forwardVelocity;
            float baseAccel = Mathf.Clamp(speedDifference / Mathf.Max(deltaTime, 0.0001f), -_config.Acceleration, _config.Acceleration);
            float slopeCompensation = 0f;
            
            if (gravityAlongForward < 0f && slopeAngle <= _maxClimbableSlopeAngle)
                slopeCompensation = -gravityAlongForward * _climbAssist;
            else if (gravityAlongForward > 0f)
                slopeCompensation = gravityAlongForward * 0.5f;

            float totalAccel = baseAccel + slopeCompensation;
            
            _rigidbody.AddForce(forwardOnGround * totalAccel, ForceMode.Acceleration);
        }
        else
        {
            float slowDownAccel = Mathf.Clamp(-forwardVelocity / Mathf.Max(deltaTime, 0.0001f), -_config.Drag, _config.Drag);
            
            _rigidbody.AddForce(forwardOnGround * slowDownAccel, ForceMode.Acceleration);
        }
        
        float turnAngleDifference = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
        float turningIntensity = hasInput ? Mathf.Clamp01(Mathf.Abs(turnAngleDifference) / 60f) : 0f;
        float effectiveLateralFriction = _config.LateralFriction * (1f - _config.DriftFactor * turningIntensity);
        float lateralCorrectionAccel = Mathf.Clamp(-lateralVelocity / Mathf.Max(deltaTime, 0.0001f), -effectiveLateralFriction, effectiveLateralFriction);
        
        _rigidbody.AddForce(rightOnGround * lateralCorrectionAccel, ForceMode.Acceleration);
        
        Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        float flatSpeed = flatVelocity.magnitude;
        float maxHorizSpeed = _config.MaxSpeed * 1.5f;
        
        if (flatSpeed > maxHorizSpeed)
        {
            Vector3 limited = flatVelocity.normalized * maxHorizSpeed;
            
            _rigidbody.velocity = new Vector3(limited.x, _rigidbody.velocity.y, limited.z);
        }
        
        float headingRate = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
        float leanFactor = Mathf.Clamp01(Mathf.Abs(headingRate) / 45f);
        float targetLean = -Mathf.Sign(headingRate) * leanFactor * _config.LeanAngleMax;
        
        _currentLeanAngle = Mathf.SmoothDamp(_currentLeanAngle, targetLean, ref _leanVelocity, _config.LeanSmoothTime, float.MaxValue, deltaTime);

        Quaternion leanQuaternion = Quaternion.AngleAxis(_currentLeanAngle, baseHeading * Vector3.forward);
        Quaternion finalRotation = leanQuaternion * baseHeading;
        
        _rigidbody.MoveRotation(finalRotation);
    }

    public void TryJump(bool jumpPressedThisFrame)
    {
        if (jumpPressedThisFrame && IsGrounded)
        {
            Vector3 jumpDir = _transform.forward;
            _jumpSystem.TryJump(_rigidbody, jumpDir, true);
        }
    }
}