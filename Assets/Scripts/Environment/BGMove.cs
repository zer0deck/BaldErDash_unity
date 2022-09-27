using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMove : MonoBehaviour
{
    private float Length, StartPos;
    public GameObject Camera;
    public float ParallaxEffect;

    void Start()
    {
        StartPos = transform.position.x;
        Length = GetComponent<SpriteRenderer>().bounds.size.x;
    }


    void Update()
    {
        float temp = (Camera.transform.position.x * (1 - ParallaxEffect));
        float dist = (Camera.transform.position.x * ParallaxEffect);

        transform.position = new Vector3(StartPos + dist, Camera.transform.position.y, transform.position.z);

        if (temp > StartPos + Length) StartPos += Length;
        else if (temp < StartPos - Length) StartPos -= Length;
    }
}
