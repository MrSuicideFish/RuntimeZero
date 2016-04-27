using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using ExitGames.Client.Photon;

public class RZWeapon_Shotgun : RZWeapon
{
    public RZWeapon_Shotgun()
        : base()
    {
        Ammo = 7;
        WeaponGraphic = Resources.Load<Sprite>("WeaponGraphics/ShotgunGraphic");
    }
}