using UnityEngine;
using System.Collections;

public class RZGameMode_Deathmatch : RZGameMode
{
    public RZGameMode_Deathmatch()
        : base()
    {
        GameModeName = "Deathmatch";

        MinimumNumPlayers = 1;
        MaximumNumPlayers = 10;
        NumOfTeams = 1;

        TimePerRound = 999999.9f;
    }

    [PunRPC]
    public override void StartRound()
    {
        base.StartRound();

        //Spawn this player
        //view.RPC("SpawnPlayer", PhotonTargets.AllBuffered, transform.position, Vector3.forward, id, PhotonNetwork.player);
        PhotonNetwork.Instantiate("Players/TestPlayer", Vector3.zero, Quaternion.identity, 0);
    }
}