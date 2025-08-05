using UnityEngine;

public class JumpSystem
{
    private readonly AudioSource _audioSource;
    private readonly AudioClip _jumpClip;
    
    private float _jumpForce;
    
    public JumpSystem(float jumpForce, AudioSource audioSource, AudioClip jumpClip)
    {
        _jumpForce = jumpForce;
        _audioSource = audioSource;
        _jumpClip = jumpClip;
    }

    public JumpSystem(float jumpForce)
    {
        _jumpForce = jumpForce;
    }
    
    public void SetJumpForce(float jumpForce)
    {
        _jumpForce = jumpForce;
    }

    public bool IsGrounded(Transform transform, float checkDistance, LayerMask layerMask)
    {
        return Physics.Raycast(transform.position, Vector3.down, out _, checkDistance, layerMask);
    }

    public void TryJump(Rigidbody rigidbody, Vector3 moveDirection, bool isGrounded)
    {
        if (!isGrounded)
            return;
       
        Vector3 velocity = rigidbody.velocity;
        velocity.y = _jumpForce;
        rigidbody.velocity = velocity;
        
        if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Vector3 horizontal = new Vector3(moveDirection.x, 0f, moveDirection.z).normalized;
            
            rigidbody.AddForce(horizontal * (_jumpForce * 0.3f), ForceMode.VelocityChange);
        }

        _audioSource?.PlayOneShot(_jumpClip);
    }
}