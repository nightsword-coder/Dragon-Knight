using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Prafabscript : MonoBehaviour
{
    public GameObject prefab;
    public Vector3 position;
    public float angle;
    public float speed;
    private GameObject example;
    private Quaternion rotation;
    void Start()
    {
        rotation = Quaternion.Euler(0, 0, angle);
        example = Instantiate(prefab, position, rotation);
    }
    void Update()
    {
        Vector3 direction = rotation*Vector3.right;
        example.transform.Translate(direction * speed * Time.deltaTime);
    }
}
