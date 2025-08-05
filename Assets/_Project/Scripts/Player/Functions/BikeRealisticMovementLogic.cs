/*using UnityEngine;

public class BikeRealisticMovementLogic
{
    private readonly Rigidbody _rigidbody;
    private readonly Transform _transform;
    private readonly PlayerConfig _config;
    private readonly JumpSystem _jumpSystem;
    
    private readonly LayerMask _layerMask;
    private readonly Transform _cameraTransform;
    private readonly Transform _visualLeanTarget;
    
    private readonly bool _hasVisualLeanTarget;
    
    private Quaternion _visualLeanBaseLocalRotation;
    private Vector3 _lastStableDesiredDirWorld = Vector3.forward;
    private Vector3 _groundNormal = Vector3.up;
    private Vector2 _rawInput;
    private Vector2 _smoothedInput;
    
    private Vector2 _lastStableRawInput = Vector2.zero;
    private float _stableInputTimer = 0f;
    private float _stableInputRequiredTime = 0.1f;
    private float _stableInputAngleThresholdDeg = 2f;
    
    private float _currentSteeringAngle = 0f;
    private float _steeringVelocity = 0f;
    private float _currentLeanAngle = 0f;
    private float _leanVelocity = 0f;
    private float _headingYawDegrees;
    private float _targetHeadingYawDegrees;
    private float _headingSmoothVelocity = 0f;
    private float _lastSteeringInput = 0f;
    private float _quickTurnTimer = 0f;
    private float _debugAccumulator = 0f;
    private float DebugInterval = 0.25f;
    private float _steeringErrorDeadzone = 0.5f;
    private float _headingAlignSpeedMin = 0.5f;
    private float _lastLateralSpeed = 0f;
    
    private bool _wasTurning = false;
    private bool _isQuickTurning = false;
    private bool _warnedNoVisualLean = false;

    public bool ForceForwardMovement { get; set; }
    public bool IsGrounded { get; private set; }

    public BikeRealisticMovementLogic(Rigidbody rigidbody, Transform transform, PlayerConfig config, JumpSystem jumpSystem, LayerMask layerMask, Transform visualLeanTarget = null, Transform cameraTransform = null)
    {
        _rigidbody = rigidbody;
        _transform = transform;
        _config = config;
        _jumpSystem = jumpSystem;
        _layerMask = layerMask;

        _visualLeanTarget = visualLeanTarget;
        _hasVisualLeanTarget = _visualLeanTarget != null;
        
        if (_hasVisualLeanTarget)
            _visualLeanBaseLocalRotation = _visualLeanTarget.localRotation;

        _headingYawDegrees = transform.eulerAngles.y;
        _targetHeadingYawDegrees = _headingYawDegrees;
        _cameraTransform = cameraTransform != null ? cameraTransform : (Camera.main ? Camera.main.transform : null);
        _lastStableDesiredDirWorld = Quaternion.Euler(0f, _headingYawDegrees, 0f) * Vector3.forward;
    }

    public void UpdateInput(IInput input)
    {
        _rawInput = input.InputDirection;
        _smoothedInput = Vector2.Lerp(_smoothedInput, _rawInput, Time.deltaTime * 8f);
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
        IsGrounded = _jumpSystem.IsGrounded(_transform, _config.JumpDistance, _layerMask) || IsGrounded;

        UpdateGroundNormal();

        Vector2 input = _smoothedInput;
        float throttleMagnitude = Mathf.Clamp01(input.magnitude);

        if (throttleMagnitude < 0.01f)
            input = Vector2.zero;

        Vector3 desiredDirWorld;
        
        if (_cameraTransform != null)
        {
            Vector3 camFwd = _cameraTransform.forward;
            camFwd.y = 0f;
            camFwd.Normalize();
            Vector3 camRight = _cameraTransform.right;
            camRight.y = 0f;
            camRight.Normalize();
            desiredDirWorld = camRight * input.x + camFwd * input.y;
        }
        else
        {
            desiredDirWorld = _transform.TransformDirection(new Vector3(input.x, 0f, input.y));
        }

        if (desiredDirWorld.sqrMagnitude > 0.0001f)
            desiredDirWorld.Normalize();
        else
            desiredDirWorld = Vector3.zero;
        
        bool hasSignificantInput = throttleMagnitude > 0.01f;
        if (hasSignificantInput && Vector2.Angle(_rawInput, _lastStableRawInput) < _stableInputAngleThresholdDeg)
        {
            _stableInputTimer += deltaTime;
        }
        else
        {
            _stableInputTimer = 0f;
            _lastStableRawInput = _rawInput;
        }
        
        bool inputDirectionStable = _stableInputTimer >= _stableInputRequiredTime;
        
        if (desiredDirWorld.sqrMagnitude > 0.001f)
        {
            if (inputDirectionStable)
            {
                _lastStableDesiredDirWorld = desiredDirWorld;
            }
            else
            {
                float t = 1f - Mathf.Exp(-_config.DesiredDirectionSmoothSpeed * deltaTime);
                _lastStableDesiredDirWorld = Vector3.Slerp(_lastStableDesiredDirWorld, desiredDirWorld, t);
                _lastStableDesiredDirWorld.Normalize();
            }
        }
        
        Vector3 stableDesired = _lastStableDesiredDirWorld;
        Vector3 currentForward = Quaternion.Euler(0f, _headingYawDegrees, 0f) * Vector3.forward;

        float angleDiff = 0f;
        
        if (stableDesired.sqrMagnitude > 0.001f)
            angleDiff = Vector3.SignedAngle(currentForward, stableDesired, Vector3.up);

        if (Mathf.Abs(angleDiff) < _config.SteeringDeadzoneDegrees)
            angleDiff = 0f;

        float steeringInput = Mathf.Clamp(angleDiff / _config.MaxSteeringAngle, -1f, 1f);
        bool isTurning = Mathf.Abs(steeringInput) > 0.01f;
        bool newTurnInitiated = false;

        if (isTurning)
        {
            if (!_wasTurning || Mathf.Sign(steeringInput) != Mathf.Sign(_lastSteeringInput))
                newTurnInitiated = true;
        }

        _wasTurning = isTurning;
        _lastSteeringInput = steeringInput;

        bool wantQuickTurn = Mathf.Abs(angleDiff) >= _config.QuickTurnAngleThreshold;

        if (wantQuickTurn && !_isQuickTurning)
        {
            _isQuickTurning = true;
            _quickTurnTimer = _config.QuickTurnDuration;
        }

        float targetSteeringAngle = steeringInput * _config.MaxSteeringAngle;

        if (newTurnInitiated && isTurning)
            _steeringVelocity += -Mathf.Sign(steeringInput) * _config.CountersteerImpulse;

        float stiffness = _config.SteeringSpringStiffness * (_isQuickTurning ? _config.QuickTurnSteeringStiffnessMultiplier : 1f);
        float minDamping = 2f * Mathf.Sqrt(stiffness);
        float damping = Mathf.Max(_config.SteeringDamping * (_isQuickTurning ? _config.QuickTurnDampingMultiplier : 1f), minDamping);
        float steeringError = _currentSteeringAngle - targetSteeringAngle;
        
        if (Mathf.Abs(steeringError) < _steeringErrorDeadzone)
            steeringError = 0f;

        float steeringAccel = -stiffness * steeringError - damping * _steeringVelocity;

        _steeringVelocity += steeringAccel * deltaTime;
        _currentSteeringAngle += _steeringVelocity * deltaTime;

        if (Mathf.Abs(targetSteeringAngle) < 0.01f && Mathf.Abs(_currentSteeringAngle) < 0.1f && Mathf.Abs(_steeringVelocity) < 0.1f)
        {
            _currentSteeringAngle = 0f;
            _steeringVelocity = 0f;
        }

        _currentSteeringAngle = Mathf.Clamp(_currentSteeringAngle, -_config.MaxSteeringAngle, _config.MaxSteeringAngle);
        
        Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        float currentSpeed = flatVelocity.magnitude;
        float rawResponse = Mathf.Clamp01(currentSpeed / _config.SteeringMinSpeedForFullResponse);
        float steeringResponseFactor = Mathf.Pow(rawResponse, _config.SteeringResponseExponent);

        if (!IsGrounded)
            steeringResponseFactor *= _config.AirSteeringFactor;

        float effectiveSteeringAngle = _currentSteeringAngle * steeringResponseFactor;

        if (Mathf.Abs(effectiveSteeringAngle) < 0.02f)
            effectiveSteeringAngle = 0f;

        float wheelBase = _config.WheelBase;
        float curvature = 0f;

        if (!Mathf.Approximately(effectiveSteeringAngle, 0f))
            curvature = Mathf.Tan(effectiveSteeringAngle * Mathf.Deg2Rad) / wheelBase;

        float headingDeltaRad = currentSpeed * curvature * deltaTime;
        float headingDeltaDeg = headingDeltaRad * Mathf.Rad2Deg;
        bool noSteeringInput = Mathf.Abs(_currentSteeringAngle) < 0.01f && !_isQuickTurning;
        
        if (hasSignificantInput && inputDirectionStable && !isTurning && stableDesired.sqrMagnitude > 0.001f)
        {
            float desiredYaw = Mathf.Atan2(stableDesired.x, stableDesired.z) * Mathf.Rad2Deg;
            
            _targetHeadingYawDegrees = desiredYaw;
            _headingSmoothVelocity = 0f;
        }
        else if (noSteeringInput && currentSpeed > _headingAlignSpeedMin)
        {
            Vector3 velDir = flatVelocity.normalized;
            
            if (velDir.sqrMagnitude > 0.001f)
            {
                float velocityYaw = Mathf.Atan2(velDir.x, velDir.z) * Mathf.Rad2Deg;
                _targetHeadingYawDegrees = velocityYaw;
            }
        }

        if (_isQuickTurning)
        {
            float yawBoost = Mathf.Sign(angleDiff) * _config.QuickTurnYawBoost * deltaTime;

            _targetHeadingYawDegrees = _headingYawDegrees + yawBoost;
            _quickTurnTimer -= deltaTime;

            if (_quickTurnTimer <= 0f)
                _isQuickTurning = false;
        }
        else if (!noSteeringInput && Mathf.Abs(effectiveSteeringAngle) > 0f)
        {
            _targetHeadingYawDegrees = _headingYawDegrees + headingDeltaDeg;
        }
        
        if (noSteeringInput && currentSpeed > 1f)
            _headingSmoothVelocity = Mathf.MoveTowards(_headingSmoothVelocity, 0f, deltaTime * 20f);

        _headingYawDegrees = Mathf.SmoothDampAngle(_headingYawDegrees, _targetHeadingYawDegrees, ref _headingSmoothVelocity, _config.HeadingSmoothTime, float.MaxValue, deltaTime);
        
        if (noSteeringInput || (hasSignificantInput && inputDirectionStable && !isTurning))
        {
            float delta = Mathf.DeltaAngle(_headingYawDegrees, _targetHeadingYawDegrees);
            
            if (Mathf.Abs(delta) < 0.1f)
            {
                _headingYawDegrees = _targetHeadingYawDegrees;
                _headingSmoothVelocity = 0f;
            }
        }
        
        Quaternion baseHeading = Quaternion.Euler(0f, _headingYawDegrees, 0f);
        
        float rawTargetLean = ComputeTargetLeanAngle(currentSpeed, effectiveSteeringAngle, wheelBase) * _config.LeanSensitivity;

        if (!IsGrounded)
            rawTargetLean *= _config.InAirLeanMultiplier;

        float targetLeanAngle = Mathf.Abs(rawTargetLean) < _config.MinLeanAngleThreshold ? 0f : rawTargetLean;
        targetLeanAngle = Mathf.Clamp(targetLeanAngle, -_config.MaxLeanAngle, _config.MaxLeanAngle);
        _currentLeanAngle = Mathf.SmoothDamp(_currentLeanAngle, targetLeanAngle, ref _leanVelocity, _config.LeanSmoothTime);
        
        Quaternion leanQuat = Quaternion.AngleAxis(_currentLeanAngle, baseHeading * Vector3.forward);
        Quaternion finalRotation = leanQuat * baseHeading;
        _rigidbody.MoveRotation(finalRotation);

        ApplyVisualLean();
        ApplyLateralFriction(deltaTime);
        ApplyForwardThrottle(throttleMagnitude, deltaTime);
        ClampSpeed();
    }

    private void ApplyVisualLean()
    {
        if (_hasVisualLeanTarget)
            _visualLeanTarget.localRotation = _visualLeanBaseLocalRotation * Quaternion.Euler(0f, 0f, _currentLeanAngle);
        else if (!_warnedNoVisualLean)
            _warnedNoVisualLean = true;
    }

    private float ComputeTargetLeanAngle(float speed, float effectiveSteeringAngleDegrees, float wheelBase)
    {
        if (speed <= 0.01f || Mathf.Approximately(effectiveSteeringAngleDegrees, 0f))
            return 0f;

        float steeringRad = effectiveSteeringAngleDegrees * Mathf.Deg2Rad;
        float curvature = Mathf.Tan(steeringRad) / wheelBase;
        
        if (Mathf.Approximately(curvature, 0f))
            return 0f;

        float turnRadius = 1f / Mathf.Abs(curvature);
        float leanRad = Mathf.Atan2(speed * speed, Physics.gravity.magnitude * turnRadius);
        
        return -Mathf.Sign(effectiveSteeringAngleDegrees) * leanRad * Mathf.Rad2Deg;
    }

    private void ApplyLateralFriction(float deltaTime)
    {
        Vector3 velocity = _rigidbody.velocity;
        Vector3 localVel = _transform.InverseTransformDirection(velocity);
        float lateralSpeed = localVel.x;

        if (Mathf.Abs(lateralSpeed) > 0.001f)
        {
            float lateralAcceleration = (lateralSpeed - _lastLateralSpeed) / Mathf.Max(deltaTime, 0.0001f);
            float dampingFactor = 0.5f; // можно вынести в конфиг при желании
            float lateralForceAmount = -lateralSpeed * _config.LateralFriction - lateralAcceleration * dampingFactor;
            Vector3 lateralForceLocal = new Vector3(lateralForceAmount, 0f, 0f);
            Vector3 lateralForceWorld = _transform.TransformDirection(lateralForceLocal);

            _rigidbody.AddForce(lateralForceWorld, ForceMode.Acceleration);
        }

        _lastLateralSpeed = lateralSpeed;
    }

    private void ApplyForwardThrottle(float throttleMagnitude, float deltaTime)
    {
        Vector3 forward = _rigidbody.rotation * Vector3.forward;
        Vector3 forwardOnSlope = Vector3.ProjectOnPlane(forward, _groundNormal).normalized;
        float forwardVel = Vector3.Dot(_rigidbody.velocity, forwardOnSlope);
        float desiredSpeed = throttleMagnitude * _config.MaxSpeed;
        float speedDiff = desiredSpeed - forwardVel;
        float throttleFactor = 1f;
        
        if (!IsGrounded && !ForceForwardMovement)
            throttleFactor = _config.AirThrottleFactor;

        float maxDeltaV = _config.Acceleration * deltaTime * throttleFactor;
        float deltaV = Mathf.Clamp(speedDiff, -maxDeltaV, maxDeltaV);
        
        _rigidbody.AddForce(forwardOnSlope * deltaV, ForceMode.VelocityChange);
        
        Vector3 gravityAlongSlope = Vector3.ProjectOnPlane(Physics.gravity, _groundNormal);
        Vector3 uphillDir = -gravityAlongSlope.normalized;
        float slopeAngle = Vector3.Angle(_groundNormal, Vector3.up);
        bool isClimbing = Vector3.Dot(forwardOnSlope, uphillDir) > 0.5f;
        
        if (isClimbing && slopeAngle <= _config.MaxClimbableSlopeAngle)
        {
            Vector3 assist = -gravityAlongSlope * _config.ClimbAssist;
            _rigidbody.AddForce(assist, ForceMode.Acceleration);
        }
    }

    private void ClampSpeed()
    {
        Vector3 flatVelocity = new Vector3(_rigidbody.velocity.x, 0f, _rigidbody.velocity.z);
        float speed = flatVelocity.magnitude;
        if (speed > _config.MaxSpeed)
        {
            Vector3 limited = flatVelocity.normalized * _config.MaxSpeed;
            _rigidbody.velocity = new Vector3(limited.x, _rigidbody.velocity.y, limited.z);
        }
    }

    public void TryJump(bool jumpPressedThisFrame)
    {
        if (jumpPressedThisFrame && IsGrounded)
        {
            Vector3 jumpDir = _transform.forward;
            _jumpSystem.TryJump(_rigidbody, jumpDir, true);
        }
    }
}*/