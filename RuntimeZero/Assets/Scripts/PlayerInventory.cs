using UnityEngine;
using System.Collections;
using Photon;

public class PlayerInventory : PunBehaviour
{
    private int InventoryIndex = 0;

    public RZWeapon CurrentWeapon { get; private set; }
    public RZWeapon[] PlayerWeapons { get; private set; }

    private PhotonView PhotonViewComponent;

    public static PlayerInventory CurrentInventory { get; private set; }

    protected virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {

            }
        }

        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {

            }
        }
    }

    void OnPhotonInstantiate( PhotonMessageInfo info )
    {
        PhotonViewComponent = GetComponent<PhotonView>();

        if ( PhotonViewComponent.isMine )
        {
            CurrentInventory = this;
        }
        else
        {

        }
    }

    [PunRPC]
    public void GiveWeapon( int weaponTypeId )
    {
        eGlobalWeaponType weaponType = (eGlobalWeaponType) weaponTypeId;

        if ( weaponType == eGlobalWeaponType.NONE)
            Debug.LogError( "Weapon ID: (" + weaponTypeId + ") doesn't exist." );
    }

    [PunRPC]
    public void SwitchWeapon(int weaponIdx)
    {
        
    }

    public void Fire(int fireModeNum)
    {
        if (CurrentWeapon != null)
        {
            
        }
    }
}