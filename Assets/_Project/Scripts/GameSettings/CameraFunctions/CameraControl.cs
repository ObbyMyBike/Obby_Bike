using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

[RequireComponent(typeof(AudioSource))]
public class CameraControl : MonoBehaviour
{
    public static bool IsPause;

    [Header("General")]
    [SerializeField] private float _sensitivity = 2;
    [SerializeField] private float _distance = 5;
    [SerializeField] private float _height = 2.3f;

    [Header("Over The Shoulder")]
    [SerializeField] private float _offsetPosition;

    [Header("Clamp Angle")]
    [SerializeField] private float _minPitch = 15f;
    [SerializeField] private float _maxPitch = 15f;

    [Header("Invert")]
    [SerializeField] private InversionX _inversionX = InversionX.Disabled;
    [SerializeField] private InversionY _inversionY = InversionY.Disabled;

    [Header("Collision")]
    [SerializeField] private LayerMask _collisionLayers;
    [SerializeField] private float _minDistance = 0.5f;
    [SerializeField] private float _sphereRadius = 0.2f;

    private Transform _playerTransform;
    private UIInfo _uiInfo;
    private AudioSource _cameraSource;
    private IUIController _uiController;

    private Vector3 _baseOffset;

    private int cameraTouchId = -1;

    private float _yaw;
    private float _pitch;

    [Inject]
    public void Construct(Player player, [Inject(Optional = true)] UIInfo uiInfo, IUIController uiController)
    {
        _playerTransform = player.PlayerController.transform;
        _uiInfo = uiInfo;
        _uiController = uiController;
        _cameraSource = GetComponent<AudioSource>();

        _baseOffset = new Vector3(_offsetPosition, _height, -_distance);

        Vector3 initialEulerAngles = transform.eulerAngles;
        _yaw = initialEulerAngles.y;
        _pitch = initialEulerAngles.x;
    }

    private void Start()
    {
        gameObject.tag = "MainCamera";

        if (_uiController != null)
        {
            _uiController.ResumeRequested += OnResumeRequested;
            _uiController.PauseRequested += OnPauseRequested;
        }
    }

    private void OnEnable()
    {
        if (_uiController != null)
        {
            _uiController.PauseRequested += OnPauseRequested;
            _uiController.ResumeRequested += OnResumeRequested;
        }
    }

    private void OnDisable()
    {
        if (_uiController != null)
        {
            _uiController.PauseRequested -= OnPauseRequested;
            _uiController.ResumeRequested -= OnResumeRequested;
        }
    }

    private void LateUpdate()
    {
        if (_playerTransform == null || IsPause)
            return;

        if (_uiInfo != null && _uiInfo.IsDown)
            return;

        UpdateCamera();
    }

    public void SetUIController(IUIController uiController)
    {
        _cameraSource = GetComponent<AudioSource>();
        _uiController = uiController;
    }

    private void UpdateCamera()
    {
        float deltaX = 0f;
        float deltaY = 0f;
        bool isRotating = false;

        if (Application.isMobilePlatform)
        {
            if (cameraTouchId == -1)
            {
                foreach (Touch touch in Input.touches)
                {
                    if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width * 0.5f)
                    {
                        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                            continue;
                        
                        cameraTouchId = touch.fingerId;
                        
                        break;
                    }
                }
            }

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.touches[i];

                if (touch.fingerId == cameraTouchId)
                {
                    if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    {
                        cameraTouchId = -1;
                        
                        break;
                    }
                    
                    if (touch.phase == TouchPhase.Moved)
                    {
                        isRotating = true;
                        deltaX = touch.deltaPosition.x;
                        deltaY = touch.deltaPosition.y;
                    }

                    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                        cameraTouchId = -1;

                    break;
                }
            }
        }
        else
        {
            isRotating = Input.GetMouseButton(0);
            
            if (isRotating && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                isRotating = false;

            if (isRotating)
            {
                deltaX = Input.GetAxis("Mouse X");
                deltaY = Input.GetAxis("Mouse Y");
            }
        }

        if (isRotating)
        {
            int invertX = _inversionX == InversionX.Disabled ? 1 : -1;
            int invertY = _inversionY == InversionY.Disabled ? -1 : 1;

            _yaw += deltaX * _sensitivity * Time.deltaTime * invertX;
            _pitch += deltaY * _sensitivity * Time.deltaTime * invertY;
            _pitch = Mathf.Clamp(_pitch, -_minPitch, _maxPitch);
        }

        Quaternion cameraRotation = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 worldOffset = cameraRotation * _baseOffset;
        Vector3 targetPosition = _playerTransform.position + worldOffset;
        Vector3 lookAtTarget = _playerTransform.position + Vector3.up * _height;

        Vector3 direction = targetPosition - lookAtTarget;
        float maxDistance = direction.magnitude;
        Ray ray = new Ray(lookAtTarget, direction.normalized);
        RaycastHit hit;

        if (Physics.SphereCast(ray, _sphereRadius, out hit, maxDistance, _collisionLayers))
        {
            float hitDistance = hit.distance;
            hitDistance = Mathf.Max(hitDistance, _minDistance);
            targetPosition = lookAtTarget + direction.normalized * hitDistance;
        }

        transform.position = targetPosition;
        transform.rotation = Quaternion.LookRotation(lookAtTarget - transform.position, Vector3.up);
    }

    private void OnPauseRequested()
    {
        IsPause = true;

        if (_cameraSource != null && _cameraSource.isPlaying)
            _cameraSource.Pause();
    }

    private void OnResumeRequested()
    {
        IsPause = false;

        if (_cameraSource != null)
            _cameraSource.UnPause();
    }
}