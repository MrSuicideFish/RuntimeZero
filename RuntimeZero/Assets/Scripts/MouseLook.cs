﻿using System;
using UnityEngine;

public class MouseLook : MonoBehaviour
{

    /* Attach Camera to Player as a child 
       Attach this script to the player
    */
     public Rigidbody CurrentWeapon;



    public float mouseSensitivity = 200.0f;
    public float clampAngle = 80.0f;

    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis
    private Rigidbody myCoreRB;
    private Rigidbody myRB;

    void Start()
    {
        Vector3 rot = transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

        myCoreRB = transform.parent.GetComponent<Rigidbody>();
        myRB = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotY = rotY + mouseX * mouseSensitivity * Time.deltaTime;
        rotX = rotX + mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        transform.rotation = localRotation;

       myRB.position = myCoreRB.position + Vector3.up + new Vector3(0, 0.1f, 0);
    }

}