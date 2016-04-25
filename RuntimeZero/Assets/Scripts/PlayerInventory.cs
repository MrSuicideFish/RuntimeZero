using UnityEngine;
using System.Collections;
using Photon;

public class PlayerInventory : PunBehaviour
{
    public int EquippedIndex { get; private set; }
    public bool IsSwitchingWeapons { get; private set; }

    public RZWeapon[] Weapons { get; private set; }

    private PhotonView PhotonViewComponent;

    public delegate void InventoryChangeDelegate(PhotonPlayer player);
    public event InventoryChangeDelegate OnPlayerSwitchWeapons;

    protected virtual void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
    {
        //Writing
        if ( stream.isWriting )
        {
            if ( PhotonNetwork.isMasterClient )
            {
                stream.SendNext(EquippedIndex);
            }
        }

        //Reading
        else if ( stream.isReading )
        {
            if ( !PhotonNetwork.isMasterClient )
            {
                EquippedIndex = (int) stream.ReceiveNext();
            }
        }
    }

    void OnPhotonInstantiate( PhotonMessageInfo info )
    {
        PhotonViewComponent = GetComponent<PhotonView>();
        Weapons = new RZWeapon[0];

        if ( PhotonViewComponent.isMine )
        {
            RZNetworkManager.LocalInventory = this;
        }
        else
        {

        }
    }

    [PunRPC]
    public void GiveWeapon( int weaponTypeId, PhotonMessageInfo msgInfo )
    {
        eGlobalWeaponType weaponType = (eGlobalWeaponType) weaponTypeId;

        if ( PhotonViewComponent.owner == msgInfo.sender )
        {
            RZWeapon[] newWeaponInv = new RZWeapon[Weapons.Length + 1];

            //Decipher weapon
            switch ( weaponType )
            {
                case eGlobalWeaponType.SHOTGUN:
                    RZWeapon_Shotgun newShotgun = gameObject.AddComponent<RZWeapon_Shotgun>();
                    newWeaponInv[newWeaponInv.Length - 1] = newShotgun;
                break;
            }
            
            Weapons = newWeaponInv;
        }

        print( "Player: " + msgInfo.sender.ID + " picked up: " + weaponType);
    }

    [PunRPC]
    public void EquipWeapon( int idx, PhotonMessageInfo msgInfo )
    {
        if ( PhotonViewComponent.owner == msgInfo.sender )
        {
            idx = idx%Weapons.Length;
            
            EquippedIndex = idx;
            IsSwitchingWeapons = true;

            if (OnPlayerSwitchWeapons != null)
            {
                OnPlayerSwitchWeapons( PhotonViewComponent.owner );
            }
        }

        print( "Player: " + msgInfo.sender.ID + " switched to weapon: " + idx );
    }
}