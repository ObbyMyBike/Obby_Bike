using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoostZone : MonoBehaviour
{
    [SerializeField] private BoostLibrary _library;
    [SerializeField] private Transform _launchPoint;
    [SerializeField] private int _presetIndex = 0;

    private BoosterSpeed _booster;
    private BounceTrampoline _trampoline;
    private CatapultLaunch _catapult;
    private BoosterJump _jumpBooster;
    private BoosterAcceleration _accelerationBooster;

    private Collider _zoneCollider;
    
    private float _lastBounceTime = -999f;
    private float _lastLaunchTime = -999f;

    private void Awake()
    {
        _zoneCollider = GetComponent<Collider>();
        _booster = new BoosterSpeed();
        _trampoline = new BounceTrampoline();
        _catapult = new CatapultLaunch();
        _jumpBooster = new BoosterJump();
        _accelerationBooster = new BoosterAcceleration();

        ApplyColliderMode();
    }

    private void FixedUpdate()
    {
        if (Current == null || Current.BoostType != BoostType.Launcher)
            return;

        if (Time.time - _lastLaunchTime < 0.2f)
            return;

        foreach (Collider hit in Physics.OverlapSphere(transform.position, Current.DetectionRadius))
        {
            if (hit.TryGetComponent(out PlayerTarget target))
            {
                _catapult.TryLaunch(target, Current, _launchPoint);
                _lastLaunchTime = Time.time;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (Current?.BoostType == BoostType.BoosterSpeed)
            HandleInteraction(other, null);
    }
    
    private void OnCollisionEnter(Collision collision)
    { 
        if (Current?.BoostType == BoostType.Trampoline)
            HandleInteraction(collision.collider, collision);
    }
    
    private void OnCollisionStay(Collision collision)
    { 
        if (Current?.BoostType == BoostType.Trampoline)
            HandleInteraction(collision.collider, collision);
    }
    
    public BoostType ZoneType => Current?.BoostType ?? BoostType.BoosterSpeed;
    
    public void ApplyBoost(PlayerTarget target)
    {
        HandleInteraction(target, null);
    }
    
    private BoostZonePreset Current => (_library != null && _presetIndex >= 0 && _presetIndex < _library.Presets.Count)
        ? _library.Presets[_presetIndex] : null;
    
    private void HandleInteraction(Component hit, Collision collision)
    {
        if (Current == null)
            return;

        if (!hit.TryGetComponent(out PlayerTarget target))
            target = hit.GetComponentInParent<PlayerTarget>();

        if (target == null)
            return;

        switch (Current.BoostType)
        {
            case BoostType.BoosterSpeed:
                _booster.TryApplyBooster(target, Current);
                break;

            case BoostType.Trampoline:
                
                if (collision != null)
                {
                    bool landingEligible = false;
                    PlayerController playerController = target.GetComponentInParent<PlayerController>();
                    
                    if (playerController != null)
                        landingEligible = playerController.LandingEligible;

                    _trampoline.TryBounce(target, Current, collision, ref _lastBounceTime, landingEligible);
                }
                
                break;

            case BoostType.Launcher:
                
                if (Time.time - _lastLaunchTime > 0.2f)
                {
                    _catapult.TryLaunch(target, Current, _launchPoint);
                    _lastLaunchTime = Time.time;
                }
                
                break;
            
            case BoostType.Acceleration:
                _accelerationBooster.TryApplyAcceleration(target, Current);
                break;

            case BoostType.Jump:
                _jumpBooster.TryApplyJumpBoost(target, Current);
                break;
        }
    }

    private void ApplyColliderMode()
    {
        if (Current == null)
            return;
        
        bool isPickupZone = Current.BoostType == BoostType.BoosterSpeed || Current.BoostType == BoostType.Acceleration || Current.BoostType == BoostType.Jump;
        
        _zoneCollider.isTrigger = isPickupZone;
    }

    private void OnDrawGizmos()
    {
        if (Current == null || Current.BoostType != BoostType.Launcher || _launchPoint == null)
            return;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, Current.DetectionRadius);
        
        float x = Current.LaunchDistanceX;
        float z = Current.LaunchDistanceZ;
        float h = Current.LandingHeight;
        float angleRad = Current.LaunchAngle * Mathf.Deg2Rad;
        float g = Physics.gravity.y;
        
        Vector3 right = _launchPoint.right;
        Vector3 forward = _launchPoint.forward;
        Vector3 hRight = new Vector3(right.x, 0f, right.z).normalized;
        Vector3 hForward = new Vector3(forward.x, 0f, forward.z).normalized;
        Vector3 offsetDir = hRight * x + hForward * z;
        float d = offsetDir.magnitude;
        
        if (d < 0.001f)
            return;
        
        Vector3 horizontalDir = offsetDir / d;
        float cosA = Mathf.Cos(angleRad);
        float sinA = Mathf.Sin(angleRad);
        float tanA = Mathf.Tan(angleRad);
        float denom = 2f * cosA * cosA * (d * tanA - h);
        
        if (denom <= 0f)
            return;
        
        float speed = Mathf.Sqrt(Mathf.Abs(g) * d * d / denom);
        Vector3 v0 = horizontalDir * (speed * cosA) + Vector3.up * (speed * sinA);
        Vector3 pos = _launchPoint.position;
        Vector3 vel = v0;
        float dt = Time.fixedDeltaTime;
        
        Gizmos.color = Color.white;
        
        int maxSteps = 200;
        
        for (int i = 0; i < maxSteps; i++)
        {
            Vector3 nextVel = vel + Physics.gravity * dt;
            Vector3 nextPos = pos + vel * dt;
            Gizmos.DrawLine(pos, nextPos);
            pos = nextPos;
            vel = nextVel;
            
            if (pos.y <= _launchPoint.position.y + h)
                break;
        }
        
        Vector3 landingPos = _launchPoint.position + horizontalDir * d + Vector3.up * h;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(landingPos, 0.3f);
    }
}