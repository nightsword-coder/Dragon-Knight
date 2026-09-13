using UnityEngine;
//bocchi
public class OrbitBalls : MonoBehaviour
{
    public GameObject ballPrefab;
    public float rotateSpeed = 90f;

    private readonly Vector3[] localOffsets =
    {
        new Vector3(-1f, 0f, 0f),
        new Vector3(0f, 1f, 0f),
        new Vector3(1f, 0f, 0f),
        new Vector3(0f, -1f, 0f)
    };

    private GameObject[] balls;
    private float angle;

    void OnEnable()
    {
        angle = 0f;
        SpawnBalls();
    }

    void OnDisable()
    {
        ReleaseBalls();
    }

    void SpawnBalls()
    {
        if (ballPrefab == null)
            return;

        if (balls == null)
            balls = new GameObject[localOffsets.Length];

        Vector3 origin = transform.position;
        for (int i = 0; i < localOffsets.Length; i++)
            balls[i] = ObjectPool.Get(ballPrefab, origin + localOffsets[i], Quaternion.identity);
    }

    void Update()
    {
        if (balls == null)
            return;

        angle += rotateSpeed * Time.deltaTime;
        Quaternion rotation = Quaternion.Euler(0f, 0f, -angle);
        Vector3 origin = transform.position;

        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i] == null || !balls[i].activeSelf)
                continue;

            Vector3 rotated = rotation * localOffsets[i];
            balls[i].transform.position = origin + rotated;
        }
    }

    public void DestroyBalls()
    {
        ReleaseBalls();
    }

    void ReleaseBalls()
    {
        if (balls == null)
            return;

        for (int i = 0; i < balls.Length; i++)
        {
            if (balls[i] != null)
            {
                ObjectPool.Release(balls[i]);
                balls[i] = null;
            }
        }
    }
}
