using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoostPickupZone : MonoBehaviour
{
    [SerializeField] private BoostZone _boostZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out PlayerTarget target))
            return;

        BoostType type = _boostZone.ZoneType;
        
        if (type == BoostType.Acceleration || type == BoostType.Jump)
        {
            _boostZone.ApplyBoost(target);
            ConsumePickup();
        }
    }
    
    private void ConsumePickup()
    {
        if (TryGetComponent(out Collider colliderObject))
            colliderObject.enabled = false;

        Destroy(gameObject);
    }
}