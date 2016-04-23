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

        if (PhotonNetwork.isMasterClient)
        {
            //spawn players randomly
            for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
            {
                Vector3 randPosition = new Vector3(Random.Range(20, 20), 5, Random.Range(20, 20));
                GameObject newPlayer = PhotonNetwork.Instantiate("Players/TestPlayer", randPosition, Quaternion.identity, 1);
                //newPlayer.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.playerList[i]);
            }
        }
    }
}