using UnityEngine;

public class Finish : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private GameObject finishBar;

    private void OnTriggerEnter(Collider other)
    {
        gameManager.Finish();
        finishBar.SetActive(true);
        
        Time.timeScale = 0;
        CameraControl.IsPause = true;
        
        if (!Application.isMobilePlatform)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}