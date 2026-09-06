using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFit1611 : MonoBehaviour
{
    private Camera cam;
    private float targetRatio = 16f / 11f; // 你的设计比例

    void Start()
    {
        cam = GetComponent<Camera>();
        UpdateCamera();
    }

    void Update()
    {
        if (Mathf.Abs(cam.aspect - targetRatio) > 0.01)
            UpdateCamera();
    }

    void UpdateCamera()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float scale = screenRatio / targetRatio;

        if (scale > 1) // 屏幕更宽 → 左右黑边
            cam.rect = new Rect((1 - 1/scale)/2, 0, 1/scale, 1);
        else // 屏幕更高 → 上下黑边
            cam.rect = new Rect(0, (1 - scale)/2, 1, scale);
    }
}