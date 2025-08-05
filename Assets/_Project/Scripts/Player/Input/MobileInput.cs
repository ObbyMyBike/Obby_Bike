using System;
using UnityEngine;
using UnityEngine.UI;

public class MobileInput : MonoBehaviour, IInput
{
    public event Action Jumped;
    public event Action Pushed;
    
    [SerializeField] private FixedJoystick fixedJoystick;
    [SerializeField] private Button _jumpingButton;
    [SerializeField] private Button _pushButton;

    private Vector2 _inputDireaction;
    public Vector2 InputDirection { get => _inputDireaction; }

    private void OnEnable()
    {
        _jumpingButton.onClick.AddListener(OnJumpClick);
        _pushButton.onClick.AddListener(OnPushClick);
    }

    private void OnDisable()
    {
        _jumpingButton.onClick.RemoveListener(OnJumpClick);
        _pushButton.onClick.RemoveListener(OnPushClick);
    }

    private void Update()
    {
        _inputDireaction = fixedJoystick.Direction;
    }

    private void OnJumpClick() => Jumped?.Invoke();
    
    private void OnPushClick() => Pushed?.Invoke();

    public void Activate() => gameObject.SetActive(true);

    public void Deactivate() => gameObject.SetActive(false);
}