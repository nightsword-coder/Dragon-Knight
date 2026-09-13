using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    readonly Dictionary<int, GameObject> instanceToPrefab = new Dictionary<int, GameObject>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject go = new GameObject("ObjectPool");
        DontDestroyOnLoad(go);
        Instance = go.AddComponent<ObjectPool>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation, bool recycleOffscreen = true)
    {
        if (prefab == null)
            return null;

        if (Instance == null)
            Bootstrap();

        return Instance.GetInternal(prefab, position, rotation, recycleOffscreen);
    }

    public static void Release(GameObject instance)
    {
        if (instance == null)
            return;

        if (Instance == null)
        {
            Destroy(instance);
            return;
        }

        Instance.ReleaseInternal(instance);
    }

    public static void Prewarm(GameObject prefab, int count)
    {
        if (prefab == null || count <= 0)
            return;

        if (Instance == null)
            Bootstrap();

        Instance.PrewarmInternal(prefab, count);
    }

    GameObject GetInternal(GameObject prefab, Vector3 position, Quaternion rotation, bool recycleOffscreen)
    {
        Queue<GameObject> queue = GetQueue(prefab);
        GameObject instance = null;

        while (queue.Count > 0)
        {
            GameObject candidate = queue.Dequeue();
            if (candidate != null)
            {
                instance = candidate;
                break;
            }
        }

        if (instance == null)
            instance = CreateInstance(prefab);

        PooledObject pooled = EnsurePooledObject(instance, prefab);
        pooled.inPool = false;
        pooled.recycleOffscreen = recycleOffscreen;

        instance.transform.SetParent(null);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        ResetPhysics(instance);
        return instance;
    }

    void ReleaseInternal(GameObject instance)
    {
        if (instance == null)
            return;

        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled != null && pooled.inPool)
            return;

        if (!instance.activeSelf && pooled != null)
            return;

        if (pooled == null || pooled.sourcePrefab == null)
        {
            Destroy(instance);
            return;
        }

        pooled.inPool = true;
        ResetPhysics(instance);
        instance.SetActive(false);
        instance.transform.SetParent(transform);
        GetQueue(pooled.sourcePrefab).Enqueue(instance);
    }

    void PrewarmInternal(GameObject prefab, int count)
    {
        Queue<GameObject> queue = GetQueue(prefab);
        for (int i = 0; i < count; i++)
        {
            GameObject instance = CreateInstance(prefab);
            EnsurePooledObject(instance, prefab).inPool = true;
            instance.SetActive(false);
            instance.transform.SetParent(transform);
            queue.Enqueue(instance);
        }
    }

    GameObject CreateInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);
        instance.name = prefab.name + "(Clone)";
        instanceToPrefab[instance.GetInstanceID()] = prefab;
        return instance;
    }

    PooledObject EnsurePooledObject(GameObject instance, GameObject prefab)
    {
        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = instance.AddComponent<PooledObject>();

        pooled.sourcePrefab = prefab;
        return pooled;
    }

    Queue<GameObject> GetQueue(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            pools[prefab] = queue;
        }
        return queue;
    }

    static void ResetPhysics(GameObject instance)
    {
        Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
