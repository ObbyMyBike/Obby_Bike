using System.Collections;
using UnityEngine;

public class CoroutineRunnerMonoBehaviour : MonoBehaviour, ICoroutineRunner
{
    public Coroutine StartCoroutine(IEnumerator routine) => base.StartCoroutine(routine);

    public void StopCoroutine(Coroutine coroutine)
    {
        if (coroutine != null)
            base.StopCoroutine(coroutine);
    }
}