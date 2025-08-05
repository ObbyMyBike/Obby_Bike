using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _percentText;
    [SerializeField] private GameObject _npcMarkerPrefab;
    [SerializeField] private RectTransform _progressBarRect;
    [SerializeField] private float _paddingX = 10f;

    private Coroutine _animationCoroutine;
    
    private float _animationDuration = 0.5f;
    
    private Dictionary<GameObject, RectTransform> _npcMarkers = new Dictionary<GameObject, RectTransform>();
    private Dictionary<GameObject, Coroutine> _npcAnimationCoroutines = new Dictionary<GameObject, Coroutine>();

    private void Awake()
    {
        _slider.minValue = 0f;
        _slider.maxValue = 1f;
        
        _slider.value = 0f;
        
        if (_percentText != null)
            _percentText.text = "0%";
    }

    public void InitializeNPC(GameObject npc)
    {
        if (!_npcMarkers.ContainsKey(npc))
        {
            GameObject marker = Instantiate(_npcMarkerPrefab, _progressBarRect);
            RectTransform markerRect = marker.GetComponent<RectTransform>();
            
            if (markerRect == null)
            {
                return;
            }
            
            Image markerImage = marker.GetComponent<Image>();
            
            if (markerImage != null)
            {
                Color randomColor = GenerateRandomColorAvoidingOrange();
                markerImage.color = randomColor;
            }

            _npcMarkers.Add(npc, markerRect);
            
            UpdateNPCMarkerPosition(npc, 0f);
        }
    }

    private Color GenerateRandomColorAvoidingOrange()
    {
        float hue = Random.value * 360f;
        
        while (hue >= 30f && hue <= 60f)
        {
            hue = Random.value * 360f;
        }
        
        float saturation = Random.Range(0.5f, 1f);
        float value = Random.Range(0.7f, 1f);
        
        return Color.HSVToRGB(hue / 360f, saturation, value);
    }

    public void AnimatePlayerProgress(float newPercent)
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        _animationCoroutine = StartCoroutine(AnimateRoutine(newPercent / 100f));
    }

    public void SetPlayerProgress(float progress)
    {
        if (_animationCoroutine != null)
            StopCoroutine(_animationCoroutine);

        float clamped = Mathf.Clamp01(progress);
        _slider.value = clamped;
        _percentText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
    }

    public void UpdateNPCProgress(GameObject npc, float progress)
    {
        if (_npcMarkers.ContainsKey(npc))
        {
            float clamped = Mathf.Clamp01(progress / 100f);
            
            if (_npcAnimationCoroutines.ContainsKey(npc) && _npcAnimationCoroutines[npc] != null)
                StopCoroutine(_npcAnimationCoroutines[npc]);
            
            _npcAnimationCoroutines[npc] = StartCoroutine(AnimateNPCMarker(npc, clamped));
        }
    }

    private void UpdateNPCMarkerPosition(GameObject npc, float normalizedProgress)
    {
        RectTransform marker = _npcMarkers[npc];
        float barWidth = _progressBarRect.rect.width;
        float effectiveWidth = barWidth - 2 * _paddingX;
        Vector2 anchoredPos = marker.anchoredPosition;
        
        anchoredPos.x = normalizedProgress * effectiveWidth - (effectiveWidth / 2) + _paddingX;
        marker.anchoredPosition = anchoredPos;
    }

    private IEnumerator AnimateNPCMarker(GameObject npc, float targetProgress)
    {
        RectTransform marker = _npcMarkers[npc];
        float barWidth = _progressBarRect.rect.width;
        float effectiveWidth = barWidth - 2 * _paddingX;
        float startX = marker.anchoredPosition.x;
        float targetX = targetProgress * effectiveWidth - (effectiveWidth / 2) + _paddingX;
        float elapsed = 0f;

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / _animationDuration);
            Vector2 anchoredPos = marker.anchoredPosition;
            anchoredPos.x = Mathf.Lerp(startX, targetX, time);
            marker.anchoredPosition = anchoredPos;

            yield return null;
        }
        
        Vector2 finalPos = marker.anchoredPosition;
        finalPos.x = targetX;
        marker.anchoredPosition = finalPos;

        _npcAnimationCoroutines[npc] = null;
    }

    private IEnumerator AnimateRoutine(float targetValue)
    {
        float startValue = _slider.value;
        float elapsed = 0f;

        _percentText.text = $"{Mathf.RoundToInt(targetValue * 100f)}%";

        while (elapsed < _animationDuration)
        {
            elapsed += Time.deltaTime;
            float time = Mathf.Clamp01(elapsed / _animationDuration);
            _slider.value = Mathf.Lerp(startValue, targetValue, time);

            yield return null;
        }

        _slider.value = targetValue;
        _animationCoroutine = null;
    }
}