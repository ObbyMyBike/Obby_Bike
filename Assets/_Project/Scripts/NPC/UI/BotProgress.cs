using UnityEngine;
using System;

public class BotProgress : MonoBehaviour
{
    public event Action<float> OnProgressUpdated;

    private Vector3 _startPosition;
    private Vector3 _finishPosition;
    private float _totalDistance;

    public void Initialize(Vector3 startPosition, Vector3 finishPosition)
    {
        _startPosition = startPosition;
        _finishPosition = finishPosition;
        _totalDistance = Vector3.Distance(startPosition, finishPosition);
        UpdateProgress();
    }

    public void UpdateProgressFromPosition(Vector3 currentPosition)
    {

        float currentDistance = Vector3.Distance(currentPosition, _finishPosition);
        float progress = _totalDistance > 0 ? (1f - currentDistance / _totalDistance) * 100f : 0f;
        
        progress = Mathf.Clamp(progress, 0f, 100f);
        
        OnProgressUpdated?.Invoke(progress);
    }

    private void UpdateProgress()
    {
        UpdateProgressFromPosition(transform.position);
    }
}