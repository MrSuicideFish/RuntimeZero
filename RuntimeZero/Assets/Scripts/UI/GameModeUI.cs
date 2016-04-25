using UnityEngine;
using System.Collections;
using Photon;
using UnityEngine.UI;

public class GameModeUI : PunBehaviour
{
    public Image WeaponGraphic,
                CrosshairGraphic;

    public Text PlayerHealthText,
                WeaponAmmoText;

    protected virtual void Start()
    {
        //subscribe to player events
        RZNetworkManager.LocalInventory.OnPlayerSwitchWeapons += OnPlayerSwitchedWeapons;
    }

    protected virtual void Update()
    {

    }

    protected void OnPlayerSwitchedWeapons(PhotonPlayer player)
    {
        if (player.isLocal)
        {
            int idx = RZNetworkManager.LocalInventory.EquippedIndex;
            SwitchWeapon(RZNetworkManager.LocalInventory.Weapons[idx].WeaponGraphic);
        }
    }

    protected virtual void SwitchWeapon(Sprite newGraphic)
    {
        WeaponGraphic.sprite = newGraphic;
    }
}
