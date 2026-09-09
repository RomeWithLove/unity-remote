using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class PooledNetworkObjectProvider : NetworkObjectProviderDefault
{
    private readonly Dictionary<NetworkPrefabId, Stack<NetworkObject>> _pools = new Dictionary<NetworkPrefabId, Stack<NetworkObject>>();

    protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
    {
        var prefabId = prefab.PrefabId;
        if (_pools.TryGetValue(prefabId, out var stack) && stack.Count > 0)
        {
            NetworkObject pooledObject = stack.Pop();
            pooledObject.gameObject.SetActive(true);
            return pooledObject;
        }

        return Object.Instantiate(prefab);
    }

    protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
    {
        if (!_pools.TryGetValue(prefabId, out var stack))
        {
            stack = new Stack<NetworkObject>();
            _pools[prefabId] = stack;
        }

        instance.gameObject.SetActive(false);
        stack.Push(instance);
    }
}