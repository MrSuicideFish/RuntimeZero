using UnityEngine;
using System.Collections;
using Photon;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : PunBehaviour
{
    /// <summary>
    /// Input Variables
    /// </summary>
    private Vector3 MoveDirection;
    private Vector3 LookDirection;

    private float LookXAngle,
                    LookYAngle;

    private Camera PlayerCamera;
    private PhotonView PhotonViewComponent;
    private CharacterController CharacterControllerComponent;

    //Camera properties
    public bool CameraBobEnabled = false,
                GravityEnabled = true;

    public float
        CameraHeight = 5.0f,
        MoveSpeed = 10.0f,
        MaxSpeedMagnitude = 1.0f,
        LookSensitivity = 1.0f,
        MinLookAngle = 0.0f,
        MaxLookAngle = 60.0f,
        CameraBobSpeed = 1.0f,
        CameraBobAmount = 1.0f;

    void Start( )
    {
        InitializePlayer( );
    }

    void InitializePlayer( )
    {
        CharacterControllerComponent = GetComponent<CharacterController>( );
        PhotonViewComponent = GetComponent<PhotonView>( );
        PlayerCamera = transform.GetChild( 0 ).GetComponent<Camera>( );
        Cursor.visible = false;
    }

    void FixedUpdate( )
    {
        //Calculate mahf
        LookXAngle += Input.GetAxis("Mouse X")*LookSensitivity*Time.deltaTime;
        LookYAngle -= Input.GetAxis("Mouse Y")*LookSensitivity*Time.deltaTime;

        //LookYAngle = Mathf.Clamp(LookXAngle, MinLookAngle, MaxLookAngle);

        /*************
        /*Locomotion
        /*************/
        //Calc dir
        Vector3 MoveDir =
            new Vector3( Input.GetAxis( "Horizontal" ), 0, Input.GetAxis( "Vertical" ) );

        //transform dir
        MoveDir = transform.TransformDirection(MoveDir);

        //Position
        if ( Vector3.Distance(Vector3.zero, CharacterControllerComponent.velocity) < MaxSpeedMagnitude)
            CharacterControllerComponent.Move( MoveDir * MoveSpeed * Time.deltaTime );

        //Rotation
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, PlayerCamera.transform.eulerAngles.y,
            transform.eulerAngles.z);

        //Process gravity (after the fact)
        if ( GravityEnabled )
        {
            CharacterControllerComponent.Move( Physics.gravity * Time.deltaTime );
        }

        /*************
        /*CAMERA
        /*************/
        var cameraOffsetPos = new Vector3( 0, CameraHeight, 0 );

        //Process camera bob
        if ( CameraBobEnabled && MoveDir != Vector3.zero )
        {
            float yOffset = Mathf.Sin(Time.time*((Mathf.PI/2) * CameraBobSpeed))*CameraBobAmount;
            cameraOffsetPos.y += yOffset;
        }

        //Position
        PlayerCamera.transform.position = transform.position + cameraOffsetPos;

        //Rotation
        PlayerCamera.transform.eulerAngles = new Vector3( LookYAngle, LookXAngle, 0 ) * Time.deltaTime;
    }
}
