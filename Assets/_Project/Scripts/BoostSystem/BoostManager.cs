using System;
using System.Collections;
using UnityEngine;

public class BoostManager
{
    public event Action<float> SpeedBoostStarted;
    public event Action SpeedBoostEnded;
    public event Action<float> JumpBoostStarted;
    public event Action JumpBoostEnded;
    
    private readonly PlayerConfig _config;
    private readonly JumpSystem _jumpSystem;
    private readonly MonoBehaviour _coroutineHost;

    private Coroutine _jumpBoostRoutine;
    private Coroutine _speedBoostRoutine;

    private float _originalSpeed;
    private float _originalJumpForce;

    public BoostManager(PlayerConfig config, JumpSystem jumpSystem, MonoBehaviour coroutineHost)
    {
        _config = config;
        _jumpSystem = jumpSystem;
        _coroutineHost = coroutineHost;
        _originalJumpForce = _config.JumpForce;
        _originalSpeed = _config.MaxSpeed;
    }

    public void ApplyJumpBoost(float newJumpForce, float duration)
    {
        if (_jumpBoostRoutine != null)
            _coroutineHost.StopCoroutine(_jumpBoostRoutine);

        _originalJumpForce = _config.JumpForce;
        _config.JumpForce = newJumpForce;
        
        _jumpSystem.SetJumpForce(newJumpForce);

        JumpBoostStarted?.Invoke(duration);
        
        _jumpBoostRoutine = _coroutineHost.StartCoroutine(JumpBoostCoroutine(duration));
    }

    public void ApplySpeedBoost(float newSpeed, float duration)
    {
        if (_speedBoostRoutine != null)
            _coroutineHost.StopCoroutine(_speedBoostRoutine);

        _originalSpeed = _config.MaxSpeed;
        _config.MaxSpeed = newSpeed;

        SpeedBoostStarted?.Invoke(duration);
        
        _speedBoostRoutine = _coroutineHost.StartCoroutine(SpeedBoostCoroutine(duration));
    }

    public void Reset()
    {
        if (_jumpBoostRoutine != null)
        {
            _coroutineHost.StopCoroutine(_jumpBoostRoutine);
            
            _jumpBoostRoutine = null;
        }

        _config.JumpForce = _originalJumpForce;
        _jumpSystem.SetJumpForce(_originalJumpForce);
        
        JumpBoostEnded?.Invoke();

        if (_speedBoostRoutine != null)
        {
            _coroutineHost.StopCoroutine(_speedBoostRoutine);
            
            _speedBoostRoutine = null;
        }

        _config.MaxSpeed = _originalSpeed;
        
        SpeedBoostEnded?.Invoke();
    }

    private IEnumerator JumpBoostCoroutine(float duration)
    {
        float timer = duration;
        
        while (timer > 0f)
        {
            yield return null;
            
            timer -= Time.deltaTime;
        }
        
        _config.JumpForce = _originalJumpForce;
        _jumpSystem.SetJumpForce(_originalJumpForce);
        _jumpBoostRoutine = null;
        
        JumpBoostEnded?.Invoke();
    }

    private IEnumerator SpeedBoostCoroutine(float duration)
    {
        float timer = duration;
        
        while (timer > 0f)
        {
            yield return null;
            
            timer -= Time.deltaTime;
        }
        
        _config.MaxSpeed = _originalSpeed;
        _speedBoostRoutine = null;
        
        SpeedBoostEnded?.Invoke();
    }
}