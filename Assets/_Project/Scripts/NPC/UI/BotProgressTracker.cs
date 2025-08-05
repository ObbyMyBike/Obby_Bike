using UnityEngine;

public class BotProgressTracker
{
    private BotProgress _botProgress;
    private ProgressBar _progressBar;
    private GameObject _npc;
    private Transform _botTransform;
    private Vector3 _startPos;
    private Vector3 _finishPos;
    private bool _initialized;

    public bool IsAlive => _npc != null;

    public BotProgressTracker(BotProgress botProgress, ProgressBar progressBar, GameObject botInstance, Vector3 startPosition, Vector3 finishPosition)
    {
        _botProgress = botProgress;
        _progressBar = progressBar;
        _npc = botInstance;
        _botTransform = botInstance.transform;
        _startPos = startPosition;
        _finishPos = finishPosition;

        _botProgress.Initialize(_startPos, _finishPos);
        _progressBar.InitializeNPC(_npc);
        
        _botProgress.OnProgressUpdated += OnProgressUpdated;

        _initialized = true;
    }

    public void Tick()
    {
        if (!_initialized || !IsAlive)
            return;
        
        _botProgress.UpdateProgressFromPosition(_botTransform.position);
    }

    private void OnProgressUpdated(float progress)
    {
        if (_progressBar != null && _npc != null)
        {
            _progressBar.UpdateNPCProgress(_npc, progress);
        }
    }

    public void Dispose()
    {
        if (_botProgress != null)
            _botProgress.OnProgressUpdated -= OnProgressUpdated;
    }
}