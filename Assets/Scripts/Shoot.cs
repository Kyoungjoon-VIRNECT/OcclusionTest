using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    public float speed = 5;
    private float lifetime = 10f;
    private Vector3 dir;

    private void Start()
    {
        dir = GameObject.Find("AR Camera").GetComponent<Camera>().transform.forward;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }
}
