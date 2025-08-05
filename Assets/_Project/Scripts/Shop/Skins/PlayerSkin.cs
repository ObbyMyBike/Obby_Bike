using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using System.Collections;
using System.Linq;

public class PlayerSkin : MonoBehaviour
{
    public UnityEvent skinEquiped;
    
    [SerializeField] private Transform _skinContainer;
    [SerializeField] private Transform _bikeContainer;
    [SerializeField] private Animator _animator;

    private AsyncOperationHandle<GameObject>? _handle;
    private bool _skinLoaded;
    
    [Inject] private Player _player;
    [Inject] private PlayerConfig _playerConfig;
    [Inject] private SkinSaver _skinSaver;

    private void Awake()
    {
        TryLoadSavedSkin();
    }

    private void OnEnable()
    {
        TryLoadSavedSkin();
    }

    private void TryLoadSavedSkin()
    {
        if (_skinLoaded)
            return;

        string savedID = _skinSaver.GetSelected();
        
        if (string.IsNullOrEmpty(savedID))
            return;
        
        SkinDefinition skin = _playerConfig.AvailableSkins.FirstOrDefault(skin => skin.name == savedID);

        if (skin != null)
        {
            _ = ApplyCharacterSkinAsync(skin);
            
            _skinLoaded = true;
        }
    }
    
    // private void Start()
    // {
    //     string savedId = _skinSaver.GetSelected();
    //     
    //     if (string.IsNullOrEmpty(savedId))
    //         return;
    //     
    //     SkinDefinition skin = _playerConfig.AvailableSkins.FirstOrDefault(skin => skin.name == savedId);
    //     
    //     if (skin != null)
    //         _ = ApplyCharacterSkinAsync(skin);
    // }
    
    public async Task ApplyCharacterSkinAsync(SkinDefinition skin)
    {
        if (_skinContainer == null)
            return;

        Transform parent = _skinContainer.parent;
        
        if (parent == null)
            return;

        Destroy(_skinContainer.gameObject);

        if (_handle.HasValue)
            Addressables.Release(_handle.Value);

        GameObject prefab = skin.Prefab;
        
        if (prefab == null && skin.PrefabReference.RuntimeKeyIsValid())
        {
            _handle = skin.PrefabReference.LoadAssetAsync<GameObject>();
            prefab = await _handle.Value.Task;
        }

        if (prefab == null)
            return;

        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            Transform child = prefab.transform.GetChild(i);
            
            if (child.GetComponentInChildren<Canvas>() != null)
                continue;
            
            GameObject instantiate = Instantiate(child.gameObject, parent, true);
            instantiate.name = child.name;

            RenameRecursively(instantiate.transform);

            instantiate.transform.localPosition = child.localPosition;
            instantiate.transform.localRotation = child.localRotation;
            instantiate.transform.localScale = child.localScale;
            
            if (i== 0)
                _skinContainer = instantiate.transform;
        }


        StartCoroutine(UpdateAnimatorCoroutine());
        
        skinEquiped?.Invoke();
    }

    public async Task ApplyBikeSkinAsync(SkinDefinition skin)
    {
        if (_bikeContainer == null)
            return;
        
        for (int i = _bikeContainer.childCount - 1; i >= 0; i--)
            Destroy(_bikeContainer.GetChild(i).gameObject);

        if (_handle.HasValue)
            Addressables.Release(_handle.Value);

        GameObject prefab = skin.bikePrefab;
        
        if (prefab == null && skin.PrefabReference.RuntimeKeyIsValid())
        {
            _handle = skin.PrefabReference.LoadAssetAsync<GameObject>();
            prefab = await _handle.Value.Task;
        }

        if (prefab == null)
            return;

        for (int i = 0; i < prefab.transform.childCount; i++)
        {
            Transform child = prefab.transform.GetChild(i);
            
            if (child.GetComponentInChildren<Canvas>() != null)
                continue;

            GameObject instantiate = Instantiate(child.gameObject, _bikeContainer, true);
            instantiate.name = child.name;

            RenameRecursively(instantiate.transform);

            instantiate.transform.localPosition = child.localPosition;
            instantiate.transform.localRotation = child.localRotation;
            instantiate.transform.localScale = child.localScale;
        }

        StartCoroutine(UpdateAnimatorCoroutine());
        skinEquiped?.Invoke();
    }

    private IEnumerator UpdateAnimatorCoroutine()
    {
        if (_animator != null)
        {
            RuntimeAnimatorController controller = _animator.runtimeAnimatorController;

            _animator.enabled = false;

            yield return new WaitForSeconds(0.01f);

            _animator.enabled = true;

            if (_animator.runtimeAnimatorController == null)
                _animator.runtimeAnimatorController = controller;

            _player.PlayerController.SetAnimator(_animator);
        }
    }

    private void RenameRecursively(Transform transform)
    {
        foreach (Transform c in transform)
            RenameRecursively(c);
    }
}