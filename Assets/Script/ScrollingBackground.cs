using UnityEngine;
public class ScrollingBackground : MonoBehaviour
{
    [Tooltip("滚动速度")]
    public float scrollSpeed;
    [Tooltip("背景图0")]
    public Transform bg0;
    [Tooltip("背景图1")]
    public Transform bg1;

    private float backgroundWidth;
    private float offset;
    //private int circle;

    void Start()
    {
        // 获取精灵宽度
        backgroundWidth = bg0.GetComponent<SpriteRenderer>().bounds.size.x;
        offset = 0f;
        //circle = 0;
    }

    void Update()
    {
        offset -= scrollSpeed * Time.deltaTime;
        if (offset < -backgroundWidth)
        {
            offset += backgroundWidth;
            //circle++;
        }
        float pos0X = offset;
        float pos1X = offset + backgroundWidth;

        bg0.position = new Vector3(pos0X, bg0.position.y, bg0.position.z);
        bg1.position = new Vector3(pos1X, bg1.position.y, bg1.position.z);

        // 图间距
        //float deltaX = Mathf.Abs(pos1X - pos0X) - backgroundWidth;
        //Debug.Log($"轮数:{circle} | bg0.x:{pos0X:F6} | bg1.x:{pos1X:F6} | abs(x1-x2)-width = {deltaX:F6}");
    }
}
