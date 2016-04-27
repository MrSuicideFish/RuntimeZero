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
    public Sprite WeaponGraphic { get; protected set; }
    public eWeaponType WeaponType = eWeaponType.NONE;
    public eWeaponAmmoType AmmoType = eWeaponAmmoType.LIMITED;

    #region Static GET
    public static RZWeapon GetWeaponByType<T> () where T : RZWeapon
    {
        T t = ScriptableObject.CreateInstance<T>();
        return t;
    }

    public static RZWeapon GetWeaponByEnum( int weapType )
    {
        eWeaponType targetType = (eWeaponType) weapType;

        switch (targetType)
        {
                case eWeaponType.SHOTGUN:
                   return ScriptableObject.CreateInstance<RZWeapon_Shotgun>();
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

    public void Fire( eWeaponFireMode fireMode = eWeaponFireMode.DEFAULT )
    {
        Debug.Log("Firing Weapon");

        if (AmmoType == eWeaponAmmoType.LIMITED)
        {
            Ammo -= 1;
            if (Ammo <= 0)
            {
                //unequip and destroy weapon
            }
        }
    }
}