using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ExitGames.Client.Photon;
using Photon;
using UnityEngine.UI;

public enum eWeaponType
{
    NONE = -1,
    MACHINE_GUN = 0,
    SHOTGUN = 1,
    ROCKET_LAUNCHER = 2
}

public enum eWeaponAmmoType
{
    LIMITED = 0,
    UNLIMITED = 1
}

public enum eWeaponFireMode
{
    DEFAULT = 0,
    ALTERNATE = 1
}

public class RZWeapon : ScriptableObject
{
    public int Ammo = 100;
    public float FireRate = 0.05f;

    public Sprite WeaponGraphic { get; protected set; }
    public eWeaponType WeaponType = eWeaponType.NONE;
    public eWeaponAmmoType AmmoType = eWeaponAmmoType.LIMITED;

    protected float ShotCooldownTime = 1.0f;
    protected bool HasFired = false;

    protected PlayerController Owner;

    #region Static GET
    public static RZWeapon GetWeaponByType<T>( ) where T : RZWeapon
    {
        T t = ScriptableObject.CreateInstance<T>( );
        return t;
    }
    public static RZWeapon GetWeaponByEnum( int weapType )
    {
        eWeaponType targetType = ( eWeaponType )weapType;

        switch ( targetType )
        {
            case eWeaponType.SHOTGUN:
                return ScriptableObject.CreateInstance<RZWeapon_Shotgun>( );
            case eWeaponType.MACHINE_GUN:
            case eWeaponType.ROCKET_LAUNCHER:
                break;
        }

        return null;
    }
    #endregion

    #region Constructor

    public RZWeapon( )
    {
        Ammo = 100;
    }

    #endregion

    #region Weapon Events
    public virtual void OnWeaponEquipped()
    {
        
    }

    public virtual void OnWeaponUnequipped()
    {
        
    }

    public virtual void OnWeaponUpdate()
    {
        //Process Shot cooldown
        if ( HasFired )
        {
            ShotCooldownTime -= Time.deltaTime * FireRate;
            if ( ShotCooldownTime <= 0 )
            {
                ShotCooldownTime = 1.0f;
                HasFired = false;
            }
        }
    }
    #endregion

    /// <summary>
    /// [RPC] Fires this weapon.
    /// </summary>
    /// <param name="fireMode"></param>
    public virtual void Fire( eWeaponFireMode fireMode = eWeaponFireMode.DEFAULT )
    {
        if ( HasFired || Ammo <= 0 ) return;

        if ( AmmoType == eWeaponAmmoType.LIMITED )
        {
            Ammo -= 1;
            if ( Ammo <= 0 )
            {

                //unequip and destroy weapon
                Owner.Inventory.DestroyWeapon( );

                return;
            }
        }

        HasFired = true;
    }

    public void RpcFireWeapon(int fireModeNum)
    {
        eWeaponFireMode mode = (eWeaponFireMode) fireModeNum;
    }
}