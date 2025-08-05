using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(menuName = "Skins/Skin Definition", fileName = "Skin")]
public class SkinDefinition : ScriptableObject
{
    [Header("Prefab (fallback / editor)")]
    public GameObject Prefab;

    [Header("Prefab (fallback / editor)")]
    public GameObject bikePrefab;

    [Header("Addressables (optional)")]
    public AssetReferenceT<GameObject> PrefabReference;
}