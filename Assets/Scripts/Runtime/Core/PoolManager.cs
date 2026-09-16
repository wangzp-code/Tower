using UnityEngine;
using System.Collections.Generic;

public class PoolManager : SingletonBase<PoolManager>
{

    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ClearAllPools();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        ClearAllPools();
    }

    private void ClearAllPools()
    {
        foreach (var pool in pools.Values)
        {
            while (pool.Count > 0)
            {
                var obj = pool.Dequeue();
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
        pools.Clear();
        prefabs.Clear();
    }

    public void Preload(GameObject prefab, string poolId, int count)
    {
        if (!prefabs.ContainsKey(poolId))
        {
            prefabs[poolId] = prefab;
            pools[poolId] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            pools[poolId].Enqueue(obj);
        }
    }

    public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolId))
        {

            return null;
        }

        GameObject obj;
        if (pools[poolId].Count > 0)
        {
            obj = pools[poolId].Dequeue();
        }
        else
        {
            obj = Instantiate(prefabs[poolId], transform);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void Despawn(string poolId, GameObject obj)
    {
        obj.SetActive(false);
        pools[poolId].Enqueue(obj);
    }
}
