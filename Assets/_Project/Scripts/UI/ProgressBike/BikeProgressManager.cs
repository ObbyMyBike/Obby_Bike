using System;
using System.Collections;
using UnityEngine;

public class BikeProgressManager
{
    public event Action<float, Sprite> ProgressUpdated;
    public event Action BikeUnlocked;
    public event Action ProgressReset;

    private readonly BikeSkinData[] _bikeSkins;
    private readonly MonoBehaviour _runner;
    private readonly int _checkpointsPerStep;
    private readonly float _progressStep;

    private float _currentProgress;
    private int _checkpointCount;
    private int _currentSkinIndex;
    private bool _isFirstUnlock = true;


    public BikeProgressManager(int checkpointsPerStep, float progressStep, BikeSkinData[] bikeSkins)
    {
        _checkpointsPerStep = checkpointsPerStep;
        _progressStep = progressStep;
        _bikeSkins = bikeSkins;
    }

    public void CheckpointReached()
    {
        _checkpointCount++;
        
        if (_checkpointCount % _checkpointsPerStep == 0)
        {
            _currentProgress += _progressStep;
            
            float clamped = Mathf.Min(_currentProgress, 100f);
            int nextIndex = _isFirstUnlock ? 0 : Mathf.Min(_currentSkinIndex + 1, _bikeSkins.Length - 1);

            Sprite uiSprite = _bikeSkins[nextIndex].BikeUISprite;
            
            ProgressUpdated?.Invoke(clamped, uiSprite);

            if (clamped >= 100f)
                Unlock();
        }
    }

    private void Unlock()
    {
        BikeUnlocked?.Invoke();

        if (!_isFirstUnlock)
            _currentSkinIndex = (_currentSkinIndex + 1) % _bikeSkins.Length;

        _isFirstUnlock = false;

        _runner.StartCoroutine(DelayedReset());
    }

    private void Reset()
    {
        _currentProgress = 0f;
        _checkpointCount = 0;

        ProgressReset?.Invoke();
    }
    
    private IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(3f);

        Reset();
    }
}