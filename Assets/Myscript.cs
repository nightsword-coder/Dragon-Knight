using UnityEngine;
public class Myscript : MonoBehaviour
{
    public float speed;
    public float angle;
    public float scale;
    void Update()
    {
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector3 direction = rotation * Vector3.right;
        transform.Translate(direction * speed * Time.deltaTime);
        transform.localScale = new Vector3(scale, scale, 0);
    }
}
