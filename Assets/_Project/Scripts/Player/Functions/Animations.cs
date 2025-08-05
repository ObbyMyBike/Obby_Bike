using UnityEngine;

public class Animations
{
    private static readonly int HashSpeed = Animator.StringToHash("Speed");
    private static readonly int HashJumpStart = Animator.StringToHash("JumpStart");
    private static readonly int HashInAir = Animator.StringToHash("InAir");
    private static readonly int HashLand = Animator.StringToHash("Land");

    private readonly Animator _animator;

    public Animations(Animator animator)
    {
        _animator = animator;
    }

    public void UpdateAnimator(Rigidbody rigidbody)
    {
        float speedParam = rigidbody.velocity.magnitude / 3f;
        
        _animator.SetFloat(HashSpeed, speedParam);
    }

    public void HandleJumpAnimation(bool wasGrounded, bool isGrounded, bool jumpPressedThisFrame)
    {
        if (wasGrounded && !isGrounded)
        {
            if (!jumpPressedThisFrame)
                _animator.SetTrigger(HashJumpStart);
            
            _animator.SetBool(HashInAir, true);
        }

        if (!wasGrounded && isGrounded)
        {
            _animator.SetTrigger(HashLand);
            _animator.SetBool(HashInAir, false);
        }
    }

    public void OnJumpTriggered()
    {
        _animator.SetTrigger(HashJumpStart);
        _animator.SetBool(HashInAir, true);
    }
}