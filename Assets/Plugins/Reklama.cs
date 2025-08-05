using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;

public class Reklama : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ShowFullscreen();

    [SerializeField] private Text wait;
    public static bool IsReclama;

    private void Start()
    {
        StartCoroutine(Reklam());
    }   
    IEnumerator Reklam()
    {
        yield return new WaitForSeconds(60f);
        wait.gameObject.SetActive(true);
        wait.text = "Реклама через 2...";
        yield return new WaitForSeconds(1f);
        wait.text = "Реклама через 1...";
        yield return new WaitForSeconds(1f);
        wait.gameObject.SetActive(false);
        IsReclama = true;

        ShowFullscreen();
        StartCoroutine(Reklam());
    }
}
