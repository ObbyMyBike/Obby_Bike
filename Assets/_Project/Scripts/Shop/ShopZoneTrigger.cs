using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShopZoneTrigger : MonoBehaviour
{
    [SerializeField] private ShopPanel _shopPanel;

    private Collider _collider;
    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    private void Reset()
    {
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            if (player!= null)
                _shopPanel.Show();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
            if (player!= null)
                _shopPanel.Hide();
    }
}