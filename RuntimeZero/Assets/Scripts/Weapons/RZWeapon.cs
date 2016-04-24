using UnityEngine;
using System.Collections;
using Photon;

public enum eGlobalWeaponType
{
    NONE = -1,
    MACHINEGUN = 0,
    SHOTGUN = 1,
    ROCKET_LAUNCHER = 2
}

public enum eWeaponAmmoType
{
    LIMITED = 0,
    UNLIMITED = 1
}

public class RZWeapon : PunBehaviour
{
    public bool IsPickedUp { get; private set; }

    public int WeaponType = 0;
    public int AmmoType = 0;
    public int AmmoCount = 0;

    private BoxCollider ColliderComponent;

    //Sync weapon with server
    protected virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {
                stream.SendNext( IsPickedUp );
                stream.SendNext( WeaponType );
                stream.SendNext( AmmoType );
                stream.SendNext( AmmoCount );
            }
        }
        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {
                IsPickedUp = ( bool )stream.ReceiveNext( );
                WeaponType = ( int )stream.ReceiveNext( );
                AmmoType = ( int )stream.ReceiveNext( );
                AmmoCount = ( int )stream.ReceiveNext( );
            }
        }
    }

    protected virtual void OnTriggerEnter(Collision other)
    {
        
    }
}