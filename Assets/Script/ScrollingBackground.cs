using UnityEngine;

public class ScrollingBackground : MonoBehaviour {
    public float scrollSpeed;  
    private float backgroundWidth;  

    void Start()
    {
        backgroundWidth = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update() {
        // 向左移动背景
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        // 如果背景完全移出屏幕，重置到右侧
        if (transform.position.x <= -backgroundWidth) {
            Vector3 resetPos = new Vector3(backgroundWidth * 2, 0, 0);
            transform.position += resetPos;
        }
    }
}