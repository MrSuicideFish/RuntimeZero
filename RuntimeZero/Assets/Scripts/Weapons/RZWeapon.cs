using UnityEngine;
using System.Collections;
using Photon;
using UnityEngine.UI;

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

public class RZWeapon : RZPickup
{
    public bool IsPickedUp { get; protected set; }
    public bool IsEquippedAndReady { get; protected set; }
    public eGlobalWeaponType WeaponType { get; protected set; }

    public Sprite WeaponGraphic;
    public Animation WeaponAnimComponent;
    public int AmmoCount = 0;

    //Extern Components
    private Image ScreenGraphic;

    //Intern Components
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
                stream.SendNext( AmmoCount );
            }
        }

        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {
                IsPickedUp = ( bool )stream.ReceiveNext( );
                AmmoCount = ( int )stream.ReceiveNext( );
            }
        }
    }

    protected override void OnPickup()
    {
        Equip();
    }

    public virtual void Equip()
    {

    }
}