using UnityEngine;
using System.Collections;
using JetBrains.Annotations;
using Photon;
using UnityEngine.UI;

public enum eDamageType
{
    MELEE   = 0,
    SHOT    = 1,
    FALL    = 2,
    BURN    = 3,
    EXPLODE = 4,
    SHOCK   = 5,
    ACID    = 6,
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : PunBehaviour
{
    //Static reference to local
    private static PlayerController LocalPlayerController;

    #region Internal Components
    public Camera PlayerCamera { get; private set; }
    public PhotonView PhotonViewComponent { get; private set; }
    public CharacterController CharacterControllerComponent { get; private set; }
    public PlayerInventory Inventory { get; private set; }
    #endregion

    #region Camera / Locomotion Propeties
    private Vector3 MoveDirection,
                    InternalForces;

    private Vector3 LookDirection,
                    VelocityVector;

    private float LookXAngle,
        LookYAngle,
        JumpHoldTimer;

    public bool CameraBobEnabled = true,
        GravityEnabled = true,
        OfflineMode = false,
        AutoPickupEnabled = true,
        HasJumped = false;

    public bool IsInitialized { get; private set; }

    public float
        CameraHeight    = 0.7f,
        MoveSpeed       = 13,
        MoveStiffness   = 3.0f,
        JumpHeight      = 5,
        JumpPower       = 0.2f,
        MaxSpeedMagnitude   = 300,
        LookSensitivity     = 6000,
        LookClampVal        = 3500,
        CameraBobSpeed      = 11,
        CameraBobAmount     = 0.07f;    

    private bool CursorLockedAndInvisible = false;
    #endregion

    #region Player Status
    public int PlayerHealth { get; private set; }
    public int PlayerArmor { get; private set; }
    #endregion

    public static PlayerController GetLocalPlayerController()
    {
        PlayerController[] allControllers = GameObject.FindObjectsOfType<PlayerController>();
        foreach (PlayerController PC in allControllers)
        {
            if (PC.PhotonViewComponent.isMine)
            {
                LocalPlayerController = PC;
                return LocalPlayerController;
            }
        }

        return null;
    }

    void Start()
    {
        if ( OfflineMode )
        {
            InitializePlayer();
        }
    }

    void OnPhotonInstantiate( PhotonMessageInfo info )
    {
        InitializePlayer( );
    }

    void InitializePlayer( )
    {
        PhotonViewComponent = GetComponent<PhotonView>( );
        CharacterControllerComponent = GetComponent<CharacterController>( );
        PlayerCamera = transform.GetChild( 0 ).GetComponent<Camera>( );
        Inventory = GetComponent<PlayerInventory>();

        if (!OfflineMode)
        {
            if (PhotonViewComponent.isMine)
            {
                //load hud
                RZNetworkManager.LocalHUD =
                    GameObject.Instantiate(Resources.Load<GameObject>(RZNetworkManager.LoadedGameMode.GameModeHUDResourcePath)).GetComponent<GameModeUI>();

                RZNetworkManager.LocalController = this;
            }
            else
            {
                PlayerCamera.enabled = false;
                this.enabled = false;
            }
        }

        IsInitialized = true;
    }

    void Update()
    {
        if ( Input.GetKeyDown( KeyCode.Space ) )
        {
            if ( CharacterControllerComponent.isGrounded )
            {
                if ( !HasJumped )
                {
                    JumpHoldTimer = 1;
                    HasJumped = true;
                }
            }
        }
    }
    
    void FixedUpdate( )
    {
        if (!IsInitialized) return;

        #region Locomotion Processing
        /*************
        /*Locomotion
        /*************/
        var inputVector = CharacterControllerComponent.isGrounded
            ? new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))*MoveSpeed*Time.deltaTime
            : new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"))*(MoveSpeed/2)*Time.deltaTime;

        if (HasJumped)
        {
            //Lerp hold timer down?
            JumpHoldTimer = Mathf.Lerp(JumpHoldTimer, 0, JumpPower );

            //Apply jump power
            VelocityVector += new Vector3( 0, JumpHeight * JumpHoldTimer, 0 ) * Time.deltaTime;

            if ( JumpHoldTimer <= 0.001f 
                || ( JumpHoldTimer <= 0.7f 
                && CharacterControllerComponent.isGrounded))
            {
                HasJumped = false;
            }
        }

        VelocityVector +=
            transform.TransformDirection( inputVector );

        //Rotation
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, PlayerCamera.transform.eulerAngles.y,
            transform.eulerAngles.z);

        //Process move
        VelocityVector -= Physics.gravity*Time.deltaTime;
        CharacterControllerComponent.Move( VelocityVector );

        VelocityVector = Vector3.Lerp(VelocityVector, Vector3.zero, MoveStiffness);
        #endregion

        #region Camera Processing
        /*************
        /*CAMERA
        /*************/
        LookXAngle += Input.GetAxis( "Mouse X" ) * LookSensitivity * Time.deltaTime;
        LookYAngle -= Input.GetAxis( "Mouse Y" ) * LookSensitivity * Time.deltaTime;

        if ( LookYAngle > LookClampVal )
            LookYAngle = LookClampVal;

        if ( LookYAngle < -LookClampVal )
            LookYAngle = -LookClampVal;

        var cameraOffsetPos = new Vector3( 0, CameraHeight, 0 );

        //Process camera bob
        if ( CameraBobEnabled 
            && CharacterControllerComponent.isGrounded 
            && CharacterControllerComponent.velocity != Vector3.zero )
        {
            float yOffset = Mathf.Sin(Time.time*((Mathf.PI/2) * CameraBobSpeed))*CameraBobAmount;
            cameraOffsetPos.y += yOffset;
        }

        //Camera Position
        PlayerCamera.transform.position = transform.position + cameraOffsetPos;

        //Camera Rotation
        PlayerCamera.transform.eulerAngles = new Vector3( LookYAngle, LookXAngle, 0 ) * Time.deltaTime;
#endregion

        /*************
        /*GAMEPLAY
        /*************/
        //CHEAT - give shotgun
        if (Input.GetKeyDown(KeyCode.I))
        {
            Inventory.GiveWeapon(eWeaponType.SHOTGUN);
        }

        //Weapon switch
        float scrollDir = Input.GetAxis("Mouse ScrollWheel");
        if ( scrollDir != 0)
        {
            int idx = (int) (Inventory.EquippedIndex + scrollDir);
            Inventory.EquipWeapon(idx);
        }

        //Fire
        if (Input.GetMouseButton( eWeaponFireMode.DEFAULT.GetHashCode() ) )
        {
            Fire( eWeaponFireMode.DEFAULT );
        }

#if UNITY_EDITOR
        //Lock / unlock mouse (DEBUG)
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            CursorLockedAndInvisible = !CursorLockedAndInvisible;

            Cursor.visible = CursorLockedAndInvisible;
            Cursor.lockState = CursorLockedAndInvisible ? CursorLockMode.Locked : CursorLockMode.None;
        }
#endif
    }

    void Fire( eWeaponFireMode fireMode = eWeaponFireMode.DEFAULT )
    {
        //Is the equipped idx within bounds?
        if (Inventory.CurrentWeapon != null)
        {
            var targetWeap = Inventory.Weapons[Inventory.EquippedIndex];

            if (targetWeap != null)
                targetWeap.Fire(fireMode);
        }
    }

    [PunRPC]
    void RpcDamagePlayer
        (
            int damage, 
            int damageType, 
            float hitPosX, 
            float hitPosY, 
            float hitPosZ, 
            PhotonMessageInfo msgInfo
        )
    {
        
    }
}
