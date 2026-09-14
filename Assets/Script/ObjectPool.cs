using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }

    readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    readonly Dictionary<int, GameObject> instanceToPrefab = new Dictionary<int, GameObject>();
    readonly Dictionary<GameObject, int> createdCounts = new Dictionary<GameObject, int>();
    readonly HashSet<GameObject> activeInstances = new HashSet<GameObject>();
    readonly List<GameObject> releaseBuffer = new List<GameObject>();

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
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        if (Instance == this)
            Instance = null;
    }

    void OnSceneUnloaded(Scene scene)
    {
        ReleaseAllActiveInternal();
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

        // Stay under the DDOL pool so scene unload cannot destroy in-flight objects.
        instance.transform.SetParent(transform, false);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        ResetPhysics(instance);
        activeInstances.Add(instance);
        return instance;
    }

    void ReleaseInternal(GameObject instance)
    {
        if (instance == null)
            return;

        activeInstances.Remove(instance);

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
        instance.transform.SetParent(transform, false);
        GetQueue(pooled.sourcePrefab).Enqueue(instance);
    }

    void ReleaseAllActiveInternal()
    {
        if (activeInstances.Count == 0)
            return;

        releaseBuffer.Clear();
        foreach (GameObject instance in activeInstances)
            releaseBuffer.Add(instance);

        for (int i = 0; i < releaseBuffer.Count; i++)
            ReleaseInternal(releaseBuffer[i]);

        releaseBuffer.Clear();
    }

    void PrewarmInternal(GameObject prefab, int count)
    {
        createdCounts.TryGetValue(prefab, out int created);
        int need = count - created;
        if (need <= 0)
            return;

        Queue<GameObject> queue = GetQueue(prefab);
        for (int i = 0; i < need; i++)
        {
            GameObject instance = CreateInstance(prefab);
            EnsurePooledObject(instance, prefab).inPool = true;
            instance.SetActive(false);
            instance.transform.SetParent(transform, false);
            queue.Enqueue(instance);
        }
    }

    GameObject CreateInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab);
        instance.SetActive(false);
        instance.name = prefab.name + "(Clone)";
        instanceToPrefab[instance.GetInstanceID()] = prefab;
        if (createdCounts.TryGetValue(prefab, out int created))
            createdCounts[prefab] = created + 1;
        else
            createdCounts[prefab] = 1;
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
