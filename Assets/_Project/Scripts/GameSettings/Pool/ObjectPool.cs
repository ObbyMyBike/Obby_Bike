using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Queue<T> _queue = new Queue<T>();
    private readonly Transform _parent;

    public ObjectPool(T prefab, int initialSize = 0, Transform parent = null)
    {
        _prefab = prefab;
        _parent = parent != null ? parent : PoolContainer.Root;
        
        for (int i = 0; i < initialSize; i++)
        {
            var instance = CreateInstance(active: false);
            _queue.Enqueue(instance);
        }
    }
    
    public T Get()
    {
        T instance;
        
        if (_queue.Count > 0)
            instance = _queue.Dequeue();
        else
            instance = CreateInstance(active: false);

        instance.transform.SetParent(_parent, worldPositionStays: true);
        instance.gameObject.SetActive(true);
        
        return instance;
    }
    
    public void Release(T instance)
    {
        instance.gameObject.SetActive(false);
        instance.transform.SetParent(_parent, worldPositionStays: true);
        _queue.Enqueue(instance);
    }
    
    private T CreateInstance(bool active)
    {
        T instance = GameObject.Instantiate(_prefab, _parent);
        
        instance.transform.SetParent(_parent, worldPositionStays: true);
        instance.gameObject.SetActive(active);
        
        return instance;
    }
}