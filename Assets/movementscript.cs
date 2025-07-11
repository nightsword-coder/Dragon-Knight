using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    
    [Header("坐标限制")]
    public bool enableBounds = true; // 是否启用边界限制
    public float minX = -10f;       // X轴最小值
    public float maxX = 10f;        // X轴最大值
    public float minY = -5f;        // Y轴最小值
    public float maxY = 5f;         // Y轴最大值

    [Header("键位设置")]
    public KeyCode upKey = KeyCode.W;
    public KeyCode downKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 输入检测
        movement.x = Input.GetKey(rightKey) ? 1 : Input.GetKey(leftKey) ? -1 : 0;
        movement.y = Input.GetKey(upKey) ? 1 : Input.GetKey(downKey) ? -1 : 0;
        movement = movement.normalized;
    }

    void FixedUpdate()
    {
        if (AbilityManager.Instance.IsAbilityAcquired(6))
        {
            moveSpeed = 6.75f;
        }
        // 计算新位置
        Vector2 newPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;
        
        // 应用坐标限制
        if (enableBounds)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);
        }
        
        // 移动角色
        rb.MovePosition(newPosition);
    }

    // 可视化调试边界（仅在编辑器中显示）
    void OnDrawGizmosSelected()
    {
        if (enableBounds)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(
                new Vector3((minX + maxX) / 2, (minY + maxY) / 2, 0),
                new Vector3(maxX - minX, maxY - minY, 1)
            );
        }
    }
}