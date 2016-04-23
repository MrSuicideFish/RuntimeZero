using UnityEngine;
using System.Collections;



public class PlayerController : MonoBehaviour
{

    /* This class will hold the data and functions for the player including:
        -Player Stats ( Hp, Armor, Movement Speed, Jump Height, Current Weapon)
        -Player Functions ( Movement control, Mouse input, Interaction command, Fire Command)
    */
<<<<<<< af8885c4b82092d91f0f4d3344be6d0e74e49f79
    Rigidbody myRB;

=======
    public Rigidbody myRB;
    public Rigidbody myHead;
    public Rigidbody CurrentWeapon;
    
>>>>>>> Updated PlayerController and MouseLook; scripts needs to be merged to PlayerController
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
<<<<<<< af8885c4b82092d91f0f4d3344be6d0e74e49f79
=======
        myHead = GameObject.Find("PlayerHead").GetComponent<Rigidbody>();
>>>>>>> Updated PlayerController and MouseLook; scripts needs to be merged to PlayerController
    }
    // Update is called once per frame
    void Update()
    {
       
      
        if (Input.GetKey("w"))
        {
            myRB.rotation = myHead.rotation;
            myRB.AddRelativeForce(Vector3.forward * PlayerMovementSpeed);
        }

        if (Input.GetKey("s"))
        {
            myRB.rotation = myHead.rotation;
            myRB.AddRelativeForce(Vector3.back * PlayerMovementSpeed);
        }
        if (Input.GetKey("a"))
        {
            myRB.rotation = myHead.rotation;
            myRB.AddRelativeForce(Vector3.left * PlayerMovementSpeed);
        }
        if (Input.GetKey("d"))
        {
            myRB.rotation = myHead.rotation;
            myRB.AddRelativeForce(Vector3.right * PlayerMovementSpeed);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            myRB.rotation = myHead.rotation;
            myRB.AddRelativeForce(Vector3.up * PlayerJumpForce);
        }
<<<<<<< af8885c4b82092d91f0f4d3344be6d0e74e49f79
        if (Input.GetMouseButton(1))
        {
           
=======
        if (Input.GetButtonDown("Fire1"))
        {
            myRB.rotation = myHead.rotation;
            Fire();
>>>>>>> Updated PlayerController and MouseLook; scripts needs to be merged to PlayerController
        }
    }


    void Fire()
    {
<<<<<<< af8885c4b82092d91f0f4d3344be6d0e74e49f79

=======
        Vector3 fwd = myHead.transform.TransformDirection(Vector3.forward);
        RaycastHit myRay;
        Debug.DrawRay(myHead.position, 200*fwd, Color.green);
    //    Debug.Log("Imma gonna Fire MY LAZer!!");

        FireRocket();

      //  if (Physics.Raycast(myRB.position, fwd, out myRay, 200000))
      //  {
            //Debug.Log("I FIRED MY LAZZER!");
        //    if (myRay.collider.gameObject.tag == "Enemy")
        //    {
             //   Debug.Log("HIT");
               // Destroy(GetComponent("Rigidbody"));
        //    }
       // }
>>>>>>> Updated PlayerController and MouseLook; scripts needs to be merged to PlayerController
    }

    void FireRocket()
    {
        Rigidbody rocketClone = (Rigidbody)Instantiate(CurrentWeapon, myHead.transform.position + (myHead.transform.forward * 2.5f), myHead.transform.rotation );
    }

}
