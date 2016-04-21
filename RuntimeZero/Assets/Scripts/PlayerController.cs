using UnityEngine;
using System.Collections;



public class PlayerController : MonoBehaviour
{

    /* This class will hold the data and functions for the player including:
        -Player Stats ( Hp, Armor, Movement Speed, Jump Height, Current Weapon)
        -Player Functions ( Movement control, Mouse input, Interaction command, Fire Command)
    */
    Rigidbody myRB;
    Transform myTrans;

    public int PlayerHp = 100;
    public int PlayerArmour = 0;
    public int PlayerMovementSpeed = 10;
    public int PlayerJumpForce = 9;
    public float PlayerFallSpeed = 9.8f;
    public int PlayerWeapon = 0;
   
    // Use this for initialization
    void Start()
    {
        myRB = GetComponent<Rigidbody>();
        myTrans = GetComponent<Transform>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("w"))
        {

            myRB.AddRelativeForce(Vector3.forward * PlayerMovementSpeed);
        }

        if (Input.GetKey("s"))
        {
            myRB.AddRelativeForce(Vector3.back * PlayerMovementSpeed);
        }
        if (Input.GetKey("a"))
        {
            myRB.AddRelativeForce(Vector3.left * PlayerMovementSpeed);
        }
        if (Input.GetKey("d"))
        {
            myRB.AddRelativeForce(Vector3.right * PlayerMovementSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myRB.AddRelativeForce(Vector3.up * PlayerJumpForce);
        }
        if (Input.GetMouseButton(0))
        {
            Fire();
        }
    }


    void Fire()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        RaycastHit myRay;
        Debug.DrawRay(myRB.position, 200*fwd, Color.green);
        Debug.Log("Imma gonna Fire MY LAZer!!");
        if (Physics.Raycast(myRB.position, fwd, out myRay, 200000))
        {
            Debug.Log("I FIRED MY LAZZER!");
            if (myRay.collider.gameObject.tag == "Enemy")
            {
                Debug.Log("HIT");
               // Destroy(GetComponent("Rigidbody"));
            }
        }
    }

    void Interact()
    {

    }
}
