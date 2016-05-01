using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using ExitGames.Client.Photon;

public class RZWeapon_Shotgun : RZWeapon
{
    public RZWeapon_Shotgun()
        : base()
    {
        Ammo            = 7;
        FireRate        = 1.5f;
        WeaponType      = eWeaponType.SHOTGUN;
        AmmoType        = eWeaponAmmoType.LIMITED;
        WeaponGraphic   = Resources.Load<Sprite>("WeaponGraphics/ShotgunGraphic");
    }

    public override void OnWeaponUpdate( )
    {
        base.OnWeaponUpdate( );
    }

    public override void Fire(eWeaponFireMode fireMode = eWeaponFireMode.DEFAULT)
    {
        base.Fire(fireMode);
    }
}