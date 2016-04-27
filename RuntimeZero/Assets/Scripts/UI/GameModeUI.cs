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
        if (RZNetworkManager.LocalController != null)
        {
            PlayerHealthText.text = "HP: " + RZNetworkManager.LocalController.PlayerHealth;

            WeaponAmmoText.text = RZNetworkManager.LocalInventory.CurrentWeapon != null
                ? "Ammo: " + RZNetworkManager.LocalInventory.Weapons[
                    RZNetworkManager.LocalInventory.EquippedIndex].Ammo
                : "";
        }
    }

    protected void OnPlayerSwitchedWeapons(PhotonPlayer player)
    {
        if (player.isLocal)
        {
            print("Local player switched Weaps UI");
            int idx = RZNetworkManager.LocalInventory.EquippedIndex;

            RZWeapon weap = RZNetworkManager.LocalInventory.Weapons[idx];
            if (weap != null)
            {
                print(weap.WeaponGraphic);
                WeaponGraphic.sprite = weap.WeaponGraphic;
                WeaponGraphic.color = Color.white;
            }
        }
    }
}
