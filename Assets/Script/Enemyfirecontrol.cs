using UnityEngine;

public class Enemyfirecontrol : MonoBehaviour
{
    //public ShieldCreate shieldSpawner; // 在Inspector中设置

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            
            // 检查护盾能力和数量
            if (ShieldCreate.Instance != null && 
                AbilityManager.Instance.IsAbilityAcquired(1) && 
                ShieldCreate.Instance.GetShieldCount() > 0)
            {
                ShieldCreate.Instance.DestroyInstance(0); // 消耗一个护盾
                return; // 护盾抵挡伤害，不扣血
            }
            
            // 扣血逻辑
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.LoseHeart();
                if (ScoreManager.Instance.Level > 5)
                {
                    playerHealth.LoseHeart();
                }
            }
        }
    }
}