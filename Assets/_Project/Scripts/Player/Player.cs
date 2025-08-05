using UnityEngine;
using Zenject;

public class Player : MonoBehaviour, IPlayer
{
    [SerializeField] private PlayerController _playerController;
    
    private CameraControl _cameraControl;
    private BoostTimerUI _boostTimerUI;
    
    [Inject]
    public void Construct(CameraControl cameraControl, BoostTimerUI boostTimerUI)
    {
        _cameraControl = cameraControl;
        _boostTimerUI = boostTimerUI;
        
        _boostTimerUI.SetPlayerController(_playerController);
    }
    
    public PlayerController PlayerController => _playerController;
    
    public void SetInput(IInput input) => _playerController.SetInput(input);

    public void Activate() => _playerController.gameObject.SetActive(true);

    public void SetUIController(IUIController uiController)
    {
        _cameraControl.SetUIController(uiController);
        _cameraControl.gameObject.SetActive(true);
    }
}