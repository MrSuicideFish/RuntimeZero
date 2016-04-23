using UnityEngine;
using System.Collections;


public class PlayerController : MonoBehaviour
{

    // Body Parts
    public Rigidbody myBody;
    public Rigidbody myHead;
    public Rigidbody CurrentWeapon;

    // Player Stats
    public int PlayerHp = 100;
    public int PlayerArmour = 0;
    public int PlayerMovementSpeed = 10;
    public int PlayerJumpForce = 9;
    public float PlayerFallSpeed = 9.8f;
    public int PlayerWeapon = 0;

    // Mouse and Head Rotation 
    public float mouseSensitivity = 200.0f;
    public float clampAngle = 80.0f;
    private float rotY = 0.0f; // rotation around the up/y axis
    private float rotX = 0.0f; // rotation around the right/x axis

    // Use this for initialization
    void Start()
    {
        //get body parts 
        myBody = GetComponent<Rigidbody>();
        myHead = GameObject.Find("PlayerHead").GetComponent<Rigidbody>();

        //get local rotation
        Vector3 rot = myHead.transform.localRotation.eulerAngles;
        rotY = rot.y;
        rotX = rot.x;

    }


    void Update()
    {
        //Mouse Look
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = -Input.GetAxis("Mouse Y");

        rotY = rotY + mouseX * mouseSensitivity * Time.deltaTime;
        rotX = rotX + mouseY * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0.0f);
        myHead.transform.rotation = localRotation;

        myHead.position = myBody.position + Vector3.up + new Vector3(0, 0.1f, 0);

        // Motion Keys
        if (Input.GetKey("w"))
        {
            myBody.rotation = myHead.rotation;
            myBody.AddRelativeForce(Vector3.forward * PlayerMovementSpeed);
        }
        if (Input.GetKey("s"))
        {
            myBody.rotation = myHead.rotation;
            myBody.AddRelativeForce(Vector3.back * PlayerMovementSpeed);
        }
        if (Input.GetKey("a"))
        {
            myBody.rotation = myHead.rotation;
            myBody.AddRelativeForce(Vector3.left * PlayerMovementSpeed);
        }
        if (Input.GetKey("d"))
        {
            myBody.rotation = myHead.rotation;
            myBody.AddRelativeForce(Vector3.right * PlayerMovementSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myBody.rotation = myHead.rotation;
            myBody.AddRelativeForce(Vector3.up * PlayerJumpForce);
        }
        // Action Keys
        if (Input.GetButtonDown("Fire1"))
        {
            myBody.rotation = myHead.rotation;
            Fire();
        }
    }


    void Fire()
    {
        // Physics based bullet  
        Rigidbody rocketClone = (Rigidbody)Instantiate(CurrentWeapon, myHead.transform.position + (myHead.transform.forward * 2.5f), myHead.transform.rotation);

        // Dumby Ray-trace
        Vector3 fwd = myHead.transform.TransformDirection(Vector3.forward);
        Debug.DrawRay(myHead.position, 2000 * fwd, Color.green);

        // Hit-scan bullet
        RaycastHit myRay;
        if (Physics.Raycast(myBody.position, fwd, out myRay, 200000))
        {
            if (myRay.collider.gameObject.tag == "Enemy")
            {
                Debug.Log("Bang");
            }
        }
    }
}
