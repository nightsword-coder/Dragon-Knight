using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 100;
    public float speed = 10f;

    public void Launch(Vector2 direction)
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            return;

        if (direction.sqrMagnitude < 0.0001f)
            direction = Vector2.right;

        rb.velocity = direction.normalized * speed;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        TryHitEnemy(collision.gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryHitEnemy(other.gameObject);
    }

    void TryHitEnemy(GameObject other)
    {
        EnemyController enemy = other.GetComponent<EnemyController>();
        if (enemy == null)
            return;

        enemy.TakeDamage(damage);
        ObjectPool.Release(gameObject);
    }
}
