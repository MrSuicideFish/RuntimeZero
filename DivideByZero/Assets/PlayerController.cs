using UnityEngine;
using System.Collections;



public class PlayerController : MonoBehaviour
{

    /* This class will hold the data and functions for the player including:
        -Player Stats ( Hp, Armor, Movement Speed, Jump Height, Current Weapon)
        -Player Functions ( Movement control, Mouse input, Interaction command, Fire Command)
    */
    Rigidbody myRB;

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
        if (Input.GetMouseButton(1))
        {
           
        }
    }


    void Fire()
    {

    }

    void Interact()
    {

    }
}
