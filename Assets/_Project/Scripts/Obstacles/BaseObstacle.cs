using UnityEngine;
using Zenject;

public abstract class BaseObstacle : MonoBehaviour
{
    [Inject] private Restart _restartPanel;
    [Inject] private DeathEffect _deathEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out BotRespawner botRespawner))
        {
            _deathEffect.PlayDieEffect(botRespawner.transform.position);
            botRespawner.Respawn();
            
            return;
        }
        
        if (!other.TryGetComponent(out PlayerController player))
            return;
        
        _deathEffect.PlayDieEffect(player.transform.position);
        
        if (other.TryGetComponent(out PlayerTarget boostTarget))
            boostTarget.StopBoost();
        
        Time.timeScale = 0f;
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
        
        other.gameObject.SetActive(false);
        _restartPanel.gameObject.SetActive(true);
        
        CameraControl.IsPause = true;
    }
}