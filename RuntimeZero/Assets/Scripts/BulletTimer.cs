 ﻿using UnityEngine;
 using System.Collections;

public class BulletTimer : MonoBehaviour
{
    public float Duration = 3.0f;
    public int BulletSpeed = 1000;

    void Start()
    {
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * BulletSpeed);
        Destroy(gameObject, Duration);
    }

}