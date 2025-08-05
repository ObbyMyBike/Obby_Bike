using UnityEngine;
using UnityEngine.UI;

public class Restart : MonoBehaviour
{
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private Pause _pause;
    [SerializeField] private GameObject _cursor;
    [SerializeField] private Button _pauseButton;
    
    private void OnEnable()
    {
        _cursor.SetActive(false);
        
        if (_pauseButton != null)
            _pauseButton.gameObject.SetActive(false);
    }
    
    public void TryRestart()
    {
        GameObject player = _gameManager.Player.PlayerController.gameObject;
        
        if (player.TryGetComponent(out PlayerTarget boostTarget))
            boostTarget.StopBoost();
        
        gameObject.SetActive(false);
        _gameManager.SetPosition();
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        
        Time.timeScale = 1;
        CameraControl.IsPause = false;
        
        if (_cursor != null)
            _cursor.SetActive(true);
        
        if (_pauseButton != null)
            _pauseButton.gameObject.SetActive(true);
    }
}