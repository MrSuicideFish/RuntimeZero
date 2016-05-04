using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using Photon;

public class PlayerInventory : PunBehaviour
{
    public int EquippedIndex = -1;
    public RZWeapon[] Weapons { get; private set; }

    public RZWeapon CurrentWeapon
    {
        get
        {
            if (EquippedIndex < 0 || EquippedIndex > Weapons.Length - 1)
                return null;

            return Weapons[EquippedIndex];
        }
    }

    public bool IsSwitchingWeapons { get; private set; }

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
        Weapons = new RZWeapon[5];

        if ( PhotonViewComponent.isMine )
        {
            RZNetworkManager.LocalInventory = this;
        }
    }

    private void Update()
    {
        if (Weapons != null)
        {
            //Process weapon updates
            for (int i = 0; i < Weapons.Length; i++)
            {
                if (Weapons[i] != null)
                {
                    Weapons[i].OnWeaponUpdate();
                }
            }
        }
    }

    public void GiveWeapon( eWeaponType weapType )
    {
        //Find weapon slot
        int openSlotIdx = -1;
        for ( int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] == null)
            {
                openSlotIdx = i;
                break;
            }else if (Weapons[i].WeaponType == weapType)
            {
                //Player already owns weapon (give ammo)
                Weapons[i].Ammo += 10;
            }
        }

        if (openSlotIdx != -1)
        {
            PhotonViewComponent.RPC("RpcGiveWeapon", PhotonTargets.AllViaServer, weapType, openSlotIdx);

            //Equip new weapon (if auto-equip enabled)
            if(RZNetworkManager.LocalController.AutoPickupEnabled)
                EquipWeapon( openSlotIdx );
        }
        else
        {
            //Not enough space in inv
        }
    }

    [PunRPC]
    public void RpcGiveWeapon( int typeIdx, int weaponSlot, PhotonMessageInfo msgInfo )
    {
        eWeaponType newWeapType = (eWeaponType) typeIdx;
        if (msgInfo.sender == PhotonViewComponent.owner)
        {
            //Give server me a weapon
            Weapons[weaponSlot] = RZWeapon.GetWeaponByEnum( typeIdx );
        }
    }

    /// <summary>
    /// Changes the player's weapon. 
    /// (This is meant as a local method, it calls it's network counter-part local processing)
    /// </summary>
    /// <param name="idx">Index of the weapon type to change to. (See RZWeapon.cs)</param>
    public void EquipWeapon( int idx )
    {
        idx = (int) Mathf.Clamp(idx, 0, Weapons.Length - 1);

        print(idx);
        EquippedIndex = idx;
        IsSwitchingWeapons = true;

        //Change weapon on network
        PhotonViewComponent.RPC( "RpcEquipWeapon", PhotonTargets.AllViaServer, EquippedIndex );
    }

    [PunRPC]
    private void RpcEquipWeapon(int idx, PhotonMessageInfo msgInfo)
    {
        if (msgInfo.sender.ID == PhotonViewComponent.owner.ID)
        {
            EquippedIndex = idx;

            if ( OnPlayerSwitchWeapons != null )
            {
                OnPlayerSwitchWeapons( PhotonViewComponent.owner );
            }
        }
    }

    public void DestroyWeapon()
    {
        
    }
}