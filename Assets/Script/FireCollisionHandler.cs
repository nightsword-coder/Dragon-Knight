using UnityEngine;

public class FireCollisionHandler : MonoBehaviour
{

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
            ObjectPool.Release(gameObject);
    }
}