using UnityEngine;

public class PooledObject : MonoBehaviour
{
    [HideInInspector] public GameObject sourcePrefab;
    [HideInInspector] public bool inPool;
    public bool recycleOffscreen = true;

    public float minX = -10f;
    public float maxX = 16f;
    public float minY = -8f;
    public float maxY = 6f;

    void Update()
    {
        if (!recycleOffscreen || inPool)
            return;

        Vector3 pos = transform.position;
        if (pos.x < minX || pos.x > maxX || pos.y < minY || pos.y > maxY)
            ObjectPool.Release(gameObject);
    }
}
